using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MegaPropertyEditor
{
	// Implement this interface to make your type shown in a custom way
	interface ICustomPropertyEdit
	{
		// If you want to override the text used for the node
		string GetEditNodeText( string strDefault );

		// If you want to show special controls on the grid then return an array from here
		// If you return null then it will fall back to the default implementation.
		Control[] CreateEditControls();

		// You'll want to position the created edit controls yourself
		// The bounds given is the space available for these controls to live in
		void PositionEditControls( Control[] controls, System.Drawing.Rectangle bounds );
	}
}
