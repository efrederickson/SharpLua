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
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
namespace SharpLua.SharpDevelop.AddIn
{

	/// <summary>
	/// Description of the pad content
	/// </summary>
	public class TestPad : AbstractPadContent
	{

		private LuaPad ctl = new LuaPad();

		/// <summary>
		/// Creates a new TestPad object
		/// </summary>
		public TestPad()
		{
			//ctl = new LuaPad()
		}

		/// <summary>
		/// The <see cref="System.Windows.Forms.Control"/> representing the pad
		/// </summary>
		public override object Control {
			get { return ctl; }
		}

		/// <summary>
		/// Refreshes the pad
		/// </summary>
		public void RedrawContent()
		{
			// TODO: Refresh the whole pad control here, renew all resource strings, whatever
			//       Note that you do not need to recreate the control.
		}

		/// <summary>
		/// Cleans up all used resources
		/// </summary>
		public override void Dispose()
		{
			ctl.Dispose();
		}
	}
}
