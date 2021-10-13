using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace TestAppForMVVMwithBaseClasses.Localization
{
    public class TranslationSourceWithMultipleResxFiles : INotifyPropertyChanged
    {
        public static TranslationSourceWithMultipleResxFiles Instance { get; } = new TranslationSourceWithMultipleResxFiles();
        private readonly Dictionary<string, ResourceManager> resourceManagerDictionary = new Dictionary<string, ResourceManager>();

        public string this[string key]
        {
            get
            {
                var (baseName, stringName) = SplitName(key);
                string? translation = null;
                if (resourceManagerDictionary.ContainsKey(baseName))
                    translation = resourceManagerDictionary[baseName].GetString(stringName, currentCulture);
                return translation ?? key;
            }
        }

        /// <summary>
        /// the culture TranslationSourceWithMultipleResxFiles uses for translations.
        /// Set this in the application by doing:
        ///     TranslationSourceWithMultipleResxFiles.Instance.CurrentCulture = new System.Globalization.CultureInfo(locale);
        /// </summary>
        private CultureInfo currentCulture = CultureInfo.InstalledUICulture;
        public CultureInfo CurrentCulture
        {
            get { return currentCulture; }
            set
            {
                if (currentCulture != value)
                {
                    currentCulture = value;
                    // string.Empty/null indicates that all properties have changed
                     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                }
            }
        }

        public void AddResourceManager(ResourceManager resourceManager)
        {
            if (!resourceManagerDictionary.ContainsKey(resourceManager.BaseName))
            {
                resourceManagerDictionary.Add(resourceManager.BaseName, resourceManager);
                var temp = resourceManagerDictionary[resourceManager.BaseName].GetResourceSet(CurrentCulture, true, true);
            }
        }

        public static (string baseName, string stringName) SplitName(string name)
        {
            int idx = name.LastIndexOf('.');
            return (name.Substring(0, idx), name.Substring(idx + 1));
        }

        // WPF bindings register PropertyChanged event if the object supports it and update themselves when it is raised
        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// In xaml you set the Translation.ResourceManager per UserContorl/Window etc.
    /// This is used so multiple resource files can be used in the application. Each child Control looks to this ResourceManager for their translations.
    /// </summary>
    public class Translation : AvaloniaObject
    {
        public static readonly AttachedProperty<ResourceManager> ResourceManagerProperty = AvaloniaProperty.RegisterAttached<Translation, AvaloniaObject, ResourceManager>("ResourceManager");

        public static ResourceManager GetResourceManager(AvaloniaObject dependencyObject)
        {
            return (ResourceManager)dependencyObject.GetValue(ResourceManagerProperty);
        }
        public static void SetResourceManager(AvaloniaObject dependencyObject, ResourceManager value)
        {
            dependencyObject.SetValue(ResourceManagerProperty, value);
        }
    }

    /// <summary>
    /// This creates the binding between the Control and the correct ResourceManager.
    /// </summary>
    public class LocExtensionWithMultipleResxFiles : MarkupExtension 
    {
        public string StringName { get; } // Key name of the translation in a resource file.

        public LocExtensionWithMultipleResxFiles(string stringName)
        {
            StringName = stringName;
        }

        // Find out what ResourceManager this control uses
        private ResourceManager? GetResourceManager(object control)
        {
            if (control is AvaloniaObject dependencyObject)
            {
                object localValue = dependencyObject.GetValue(Translation.ResourceManagerProperty);

                // does this control have a "Translation.ResourceManager" attached property with a set value?
                if (localValue != AvaloniaProperty.UnsetValue)
                {
                    if (localValue is ResourceManager resourceManager)
                    {
                        TranslationSourceWithMultipleResxFiles.Instance.AddResourceManager(resourceManager);
                        return resourceManager;
                    }
                }
            }
            return null;
        }

        // Create a binding between the Control and the translated text in a resource file.
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // targetObject is the control that is using the LocExtensionWithMultipleResxFiles
            object? targetObject = (serviceProvider as IProvideValueTarget)?.TargetObject;

            if (targetObject?.GetType().Name == "SharedDp") // is extension used in a control template?
                return targetObject; // required for template re-binding

            string baseName = GetResourceManager(targetObject)?.BaseName ?? string.Empty; // if the targetObject has a ResourceManager set, BaseName is set

            // if the targetobjest doesnt have a RM set, it gets the root elements RM.
            if (string.IsNullOrEmpty(baseName))
            {
                // rootObject is the root control of the visual tree (the top parent of targetObject)
                object? rootObject = (serviceProvider as IRootObjectProvider)?.RootObject;
                baseName = GetResourceManager(rootObject)?.BaseName ?? string.Empty;
            }

            if (string.IsNullOrEmpty(baseName)) // template re-binding
            {
                if (targetObject is Control frameworkElement)
                    baseName = GetResourceManager(frameworkElement.TemplatedParent)?.BaseName ?? string.Empty;
            }

            // create a binding between the Control on the view and the correct resource-file
            var binding = new Binding
            {
                Mode = BindingMode.OneWay,
                Path = $"[{baseName}.{StringName}]", // This is the ResourceManager.Key
                Source = TranslationSourceWithMultipleResxFiles.Instance,
                FallbackValue = "Fallback, can't set translation.",
                TargetNullValue = StringName,
            };
            return binding;
        }
    }
}
