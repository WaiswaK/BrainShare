﻿

#pragma checksum "D:\Work\BrainShare Current Work\BrainShare 7.0.0.0\BrainShare\Views\TopicPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "15A762B30756B50479210DFA3CABC5E2"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BrainShare.Views
{
    partial class TopicPage : global::Windows.UI.Xaml.Controls.Page, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 94 "..\..\..\Views\TopicPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Selector)(target)).SelectionChanged += this.itemGridView_SelectionChanged;
                 #line default
                 #line hidden
                #line 94 "..\..\..\Views\TopicPage.xaml"
                ((global::Windows.UI.Xaml.Controls.ListViewBase)(target)).ItemClick += this.File_click;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 70 "..\..\..\Views\TopicPage.xaml"
                ((global::Windows.UI.Xaml.FrameworkElement)(target)).Loaded += this.WebView2_Loaded;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}

