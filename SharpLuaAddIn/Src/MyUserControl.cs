using System;
using System.Collections;
using System.Windows.Forms;
//
// Created by SharpDevelop.
// User: elijah
// Date: 05/20/2011
// Time: 8:45 PM
// 
// To change this template use Tools | Options | Coding | Edit Standard Headers.
//
using ICSharpCode.SharpDevelop.Gui.XmlForms;
using System.IO;
using System.Reflection;
namespace SharpLua.SharpDevelop.AddIn
{

	public class LuaPad : BaseSharpDevelopUserControl
	{

		public LuaPad()
		{
			System.IO.Stream StreamX = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SharpLua.SharpDevelop.AddIn.Pad.xfrm");
			SetupFromXmlStream(StreamX);
			//AddHandler (Me.Get(Of Button)("test")).Click, AddressOf ButtonClick
			SetupWebPage();
		}

		public void SetupWebPage()
		{
			WebBrowser WebBrowser = new WebBrowser();
			WebBrowser.ScrollBarsEnabled = true;
			WebBrowser.ScriptErrorsSuppressed = true;
			WebBrowser.Parent = this;
			WebBrowser.Location = new System.Drawing.Point(19, 100);
			WebBrowser.Size = new System.Drawing.Size(this.Size.Width, this.Size.Height * 3);
			WebBrowser.Dock = DockStyle.Fill;
			WebBrowser.Navigate("http://www.lua.org/about.html");
		}
	}
}
