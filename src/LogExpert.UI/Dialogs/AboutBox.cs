﻿using LogExpert.Core.Classes;

using Newtonsoft.Json;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
public partial class AboutBox : Form
{
    #region Fields

    private readonly Assembly _assembly;

    #endregion

    #region cTor

    public AboutBox()
    {
        InitializeComponent();

        LoadResources();

        _assembly = Assembly.GetExecutingAssembly();

        Text = $@"About {AssemblyTitle}";
        labelProductName.Text = AssemblyProduct;
        labelVersion.Text = AssemblyVersion;
        labelCopyright.Text = AssemblyCopyright;
        string link = "https://github.com/LogExperts/LogExpert";
        linkLabelURL.Links.Add(new LinkLabel.Link(0, link.Length, link));
        LoadUsedComponents();
    }

    //Name, Version, License, Download, Source

    private void LoadUsedComponents()
    {
        string json = File.ReadAllText($"{Application.StartupPath}files\\json\\usedComponents.json");
        var usedComponents = JsonConvert.DeserializeObject<UsedComponents[]>(json);
        usedComponents = usedComponents?.OrderBy(x => x.PackageId).ToArray();
        usedComponentsDataGrid.DataSource = usedComponents;
    }


    private void LoadResources()
    {
        logoPictureBox.Image = Resources.Resources.LogLover;
    }

    #endregion

    #region Properties

    public string AssemblyTitle
    {
        get
        {
            object[] attributes = _assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
            {
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                if (titleAttribute.Title != string.Empty)
                {
                    return titleAttribute.Title;
                }
            }
            return Path.GetFileNameWithoutExtension(_assembly.Location);
        }
    }

    public string AssemblyVersion
    {
        get
        {
            AssemblyName assembly = _assembly.GetName();

            if (assembly.Version != null)
            {
                string version = $"{assembly.Version.Major}.{assembly.Version.Minor}.{assembly.Version.Build}.{assembly.Version.Revision}";
                if (assembly.Version.Revision >= 9000)
                {
                    version += " Testrelease";
                }

                return version;
            }

            return "0.0.0.0";
        }

    }

    public string AssemblyDescription
    {
        get
        {
            object[] attributes = _assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);

            if (attributes.Length == 0)
            {
                return string.Empty;
            }
            return ((AssemblyDescriptionAttribute)attributes[0]).Description;
        }
    }

    public string AssemblyProduct
    {
        get
        {
            object[] attributes = _assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }
            return ((AssemblyProductAttribute)attributes[0]).Product;
        }
    }

    public string AssemblyCopyright
    {
        get
        {
            object[] attributes = _assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length == 0)
            {
                return string.Empty;
            }
            return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }
    }

    #endregion

    #region Events handler

    private void OnLinkLabelURLClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        string? target = string.Empty;

        if (e.Link != null)
        {
            target = e.Link.LinkData as string;
        }

        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = target,
        });
    }

    #endregion
}