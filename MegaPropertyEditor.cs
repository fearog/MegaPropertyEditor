﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;

namespace MegaPropertyEditor
{
	public partial class MegaPropertyEditor : UserControl
	{
		private Object m_link;
		private Dictionary< PropertyToken, BasicNode > m_objectNodes;
		private List<BasicNode> m_usableNodes;
		private Dictionary<Type, List<Type>> m_typesDerived;
		private int m_iMaxTreeWidth;
		//--------------------------------------------------------------------

		public MegaPropertyEditor()
		{
			InitializeComponent();

			m_objectNodes = new Dictionary<PropertyToken, BasicNode>();
			m_typesDerived = new Dictionary<Type, List<Type>>();
			m_usableNodes = new List<BasicNode>();
		}
		//--------------------------------------------------------------------

		private class PropertyToken : IEquatable<PropertyToken>
		{
			public PropertyInfo m_property;
			public object m_object;
			public int m_iElementIndex;			// This is only used by elements of arrays, which are kind of "virtual" properties
			public PropertyToken m_parent;

			public Type m_elementType;
			
			// TODO: Add index in parent's list, so we can sort these correctly!
			// TODO: Add reference to element's type
			
			public PropertyToken( PropertyInfo prop, object propOwner, int iElementIndex = -1 )
			{
				m_property = prop;
				m_object = propOwner;
				m_iElementIndex = iElementIndex;
			}

			public override bool Equals( object obj )
			{
				return this.Equals( obj as PropertyToken );
			}

			public bool Equals( PropertyToken p )
			{
				// If parameter is null, return false. 
				if( Object.ReferenceEquals( p, null ) )
				{
					return false;
				}

				// Optimization for a common success case. 
				if( Object.ReferenceEquals( this, p ) )
				{
					return true;
				}

				// If run-time types are not exactly the same, return false. 
				if( this.GetType() != p.GetType() )
					return false;

				// Return true if the fields match. 
				// Note that the base class is not invoked because it is 
				// System.Object, which defines Equals as reference equality. 

				return ( m_property == p.m_property ) 
					&& ( m_object == p.m_object ) 
					&& ( m_iElementIndex == p.m_iElementIndex )
					&& ( m_parent == p.m_parent )
					&& ( m_elementType == p.m_elementType );
			}

			public override int GetHashCode()
			{
			//	return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode( this );
				unchecked
				{
					var result = 0;
					result = ( result * 397 ) ^ m_property.GetHashCode();
					result = ( result * 397 ) ^ m_object.GetHashCode();
					result = ( result * 397 ) ^ m_iElementIndex.GetHashCode();
					if( m_parent != null )
						result = ( result * 397 ) ^ m_parent.GetHashCode();
					if( m_elementType != null )
						result = ( result * 397 ) ^ m_elementType.GetHashCode();
					return result;
				}
			}

			public static bool operator ==( PropertyToken lhs, PropertyToken rhs )
			{
				// Check for null on left side. 
				if( Object.ReferenceEquals( lhs, null ) )
				{
					if( Object.ReferenceEquals( rhs, null ) )
					{
						// null == null = true. 
						return true;
					}

					// Only the left side is null. 
					return false;
				}
				// Equals handles case of null on right side. 
				return lhs.Equals( rhs );
			}

			public static bool operator !=( PropertyToken lhs, PropertyToken rhs )
			{
				return !( lhs == rhs );
			}
		}
		//--------------------------------------------------------------------

		private class BasicNode : TreeNode
		{
			protected MegaPropertyEditor m_grid;
			protected PropertyToken m_token;

			private TextBox m_editBox;

			public BasicNode( MegaPropertyEditor grid, PropertyToken token, string str )
				: base( str )
			{
				m_grid = grid;
				m_token = token;
			}

			~BasicNode()
			{
				HideEditControls();
			}


			public virtual void ShowEditControls()
			{
				if( !m_token.m_property.PropertyType.IsPrimitive )
					return;

				// create and add a button to insert elements
				if( m_editBox == null )
				{
					string value = m_token.m_property.GetValue( m_token.m_object, null ).ToString();
					m_editBox = new TextBox();
					m_editBox.Text = value;
					PositionEditControls();
					m_editBox.BringToFront();
					m_editBox.BorderStyle = BorderStyle.FixedSingle;
					m_editBox.TextChanged += OnTextChanged;
					m_grid.m_ctlTreeView.Controls.Add( m_editBox );
				}
			}

			public virtual void PositionEditControls()
			{
				if( m_editBox != null )
				{
					m_editBox.Location = new System.Drawing.Point( m_grid.m_iMaxTreeWidth + 16, Bounds.Top + 2 );
					m_editBox.Size = new System.Drawing.Size( m_grid.m_ctlTreeView.Bounds.Width - m_editBox.Location.X, Bounds.Height );
				}
			}

			public virtual void HideEditControls()
			{
				if( m_editBox != null )
				{
					m_grid.m_ctlTreeView.Controls.Remove( m_editBox );
					m_editBox = null;
				}
			}

			public void OnTextChanged( object sender, EventArgs e )
			{
				try
				{
					m_token.m_property.SetValue( m_token.m_object, Convert.ChangeType( m_editBox.Text, m_token.m_property.PropertyType ), null );

					m_editBox.BackColor = Color.White;
				}
				catch( System.FormatException )
				{
					// highlight red if the value is no good
					m_editBox.BackColor = Color.IndianRed;
				}
			}
		}
		//--------------------------------------------------------------------
/*
		private class ClassNode : BasicNode
		{

			public ClassNode( MegaPropertyEditor grid, PropertyToken token, string str )
				: base( grid, token, str )
			{
			}
		}
		//--------------------------------------------------------------------
		*/

		private List<Type> GetDerivedTypes( Type type )
		{
			if( !m_typesDerived.ContainsKey( type ) )
			{
				List<Type> suitableTypes = new List<Type>();

				if( !type.IsPrimitive )
				{
					// Discover any and all types that may be used here
					foreach( Assembly a in AppDomain.CurrentDomain.GetAssemblies() )
					{
						foreach( Type t in a.GetTypes() )
						{
							if( type.IsAssignableFrom( t ) && t.GetConstructor( Type.EmptyTypes ) != null )
								suitableTypes.Add( t );
						}
					}
				}
				else
				{
					suitableTypes.Add( type );
				}

				m_typesDerived[ type ] = suitableTypes;
			}
			
			return m_typesDerived[ type ];
		}
		//--------------------------------------------------------------------

		private class ArrayNode : BasicNode
		{
			private Button m_addButton;
			private ComboBox m_typeSelector;
			private IList m_list;
			private List<Type> m_suitableTypes;

			public ArrayNode( MegaPropertyEditor grid, PropertyToken token, string str )
				: base( grid, token, str )
			{
				m_list = token.m_property.GetValue( token.m_object, null ) as IList;
				Type type = m_list.GetType().GetGenericArguments()[ 0 ];
				m_suitableTypes = m_grid.GetDerivedTypes( type );
			}

			public override void ShowEditControls()
			{
				// create and add a button to insert elements
				if( m_addButton == null )
				{
					m_typeSelector = new ComboBox();
					foreach( Type t in m_suitableTypes )
						m_typeSelector.Items.Add( t.Name );
					m_typeSelector.SelectedIndex = 0;

					m_addButton = new Button();
					m_addButton.Text = "Insert";
					m_addButton.FlatStyle = FlatStyle.System;

					PositionEditControls();

					m_typeSelector.BringToFront();
					m_addButton.BringToFront();
					m_addButton.Click += OnAddClicked;
					m_grid.m_ctlTreeView.Controls.Add( m_typeSelector );
					m_grid.m_ctlTreeView.Controls.Add( m_addButton );
				}
			}

			public override void PositionEditControls()
			{
				if( m_typeSelector != null )
				{
					m_typeSelector.Location = new System.Drawing.Point( Bounds.Left, Bounds.Top );
					m_typeSelector.Size = new System.Drawing.Size( m_grid.m_ctlTreeView.Bounds.Width - m_typeSelector.Location.X - 50, Bounds.Height );
					m_addButton.Location = new System.Drawing.Point( m_typeSelector.Right + 8, Bounds.Top );
					m_addButton.Size = new System.Drawing.Size( m_grid.m_ctlTreeView.Bounds.Width - m_addButton.Location.X, Bounds.Height );
				}
			}

			public override void HideEditControls()
			{
				if( m_typeSelector != null )
				{
					m_grid.m_ctlTreeView.Controls.Remove( m_typeSelector );
					m_grid.m_ctlTreeView.Controls.Remove( m_addButton );
					m_typeSelector = null;
					m_addButton = null;
				}
			}

			public void OnAddClicked( object sender, EventArgs e )
			{
				// find and remove ourselves from the list
				object newObject = System.Activator.CreateInstance( m_suitableTypes[ m_typeSelector.SelectedIndex ] );
				m_list.Add( newObject );
				m_grid.Dirty();
			}
		}
		//--------------------------------------------------------------------

		private class ArrayElementNode : BasicNode
		{
			private Button m_eraseButton;
			private IList m_list;

			public ArrayElementNode( MegaPropertyEditor grid, PropertyToken token, string str )
				: base( grid, token, str )
			{
				m_list = token.m_property.GetValue( token.m_object, null ) as IList;

				Type type = m_list.GetType().GetGenericArguments()[ 0 ];
				if( m_grid.GetDerivedTypes( type ).Count != 1 )
				{
					// can be different types, so lets change the name
					object element = m_list[ token.m_iElementIndex ];
					Text = Text + " " + element.GetType().Name;
				}
			}

			public override void ShowEditControls()
			{
				// create and add a button to insert elements
				if( m_eraseButton == null )
				{
					m_eraseButton = new Button();
					m_eraseButton.Text = "X";
					PositionEditControls();
					m_eraseButton.BringToFront();
					m_eraseButton.FlatStyle = FlatStyle.System;
					
					m_eraseButton.Click += OnEraseClicked;
					m_grid.m_ctlTreeView.Controls.Add( m_eraseButton );
				}
			}

			public override void PositionEditControls()
			{
				if( m_eraseButton != null )
				{
					m_eraseButton.Location = new System.Drawing.Point( m_grid.m_ctlTreeView.Bounds.Width - 20, Bounds.Top );
					m_eraseButton.Size = new System.Drawing.Size( m_grid.m_ctlTreeView.Bounds.Width - m_eraseButton.Location.X, Bounds.Height );
				}
			}

			public override void HideEditControls()
			{
				if( m_eraseButton != null )
				{
					m_grid.m_ctlTreeView.Controls.Remove( m_eraseButton );
					m_eraseButton = null;
				}
			}

			public void OnEraseClicked( object sender, EventArgs e )
			{
				// find and remove ourselves from the list
				m_list.RemoveAt( m_token.m_iElementIndex );
				m_grid.Dirty();
			}
		}
		//--------------------------------------------------------------------
	
		private void CreateListTokens( object arrayObject, PropertyToken arrayToken, List<PropertyToken> tokensFlatList )
		{
			// Need to make a brand new list of nodes
			IList list = ( IList )arrayObject;
			int i = 0;
			foreach( object v in list )
			{
				PropertyToken elementToken = new PropertyToken( arrayToken.m_property, arrayToken.m_object, i );
				elementToken.m_parent = arrayToken;
				elementToken.m_elementType = v.GetType();
				tokensFlatList.Add( elementToken );
				++i;

				BuildPropertyTokenTree( v, elementToken, tokensFlatList );
			}

			// magic element to become the array element adding thing
			PropertyToken adderToken = new PropertyToken( arrayToken.m_property, arrayToken.m_object, -2 );
			adderToken.m_parent = arrayToken;
			tokensFlatList.Add( adderToken );
		}
		//--------------------------------------------------------------------

		public bool IsTypeAList( Type type )
		{
			var interfaceTest = new Predicate< Type >( i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof( IList<> ) );
			return interfaceTest( type ) || type.GetInterfaces().Any( i => interfaceTest( i ) );
		}
		//--------------------------------------------------------------------

		private void BuildPropertyTokenTree( Object o, PropertyToken parentToken, List<PropertyToken> tokensFlatList )
		{
			Type oType = o.GetType();
			foreach( PropertyInfo prop in oType.GetProperties() )
			{
				if( prop.CanRead )
				{
					PropertyToken token = new PropertyToken( prop, o );
					token.m_parent = parentToken;
					tokensFlatList.Add( token );

					object value = prop.GetValue( o, null );
					
					if( IsTypeAList( prop.PropertyType ) )
					{
						if( value != null )
							CreateListTokens( value, token, tokensFlatList );
					}
					else if( prop.PropertyType.IsClass )
					{
						if( value != null )
							BuildPropertyTokenTree( value, token, tokensFlatList );
					}
				}
			}
		}
		//--------------------------------------------------------------------

		public void SetItem( Object o )
		{
			m_link = o;
			if( m_link != null )
			{
				List<PropertyToken> tokensFlatList = new List<PropertyToken>();
				BuildPropertyTokenTree( o, null, tokensFlatList );

				// find out what tokens need to be added
				List<PropertyToken> tokensToAdd = new List<PropertyToken>();
				foreach( PropertyToken t in tokensFlatList )
				{
					if( !m_objectNodes.ContainsKey( t ) )
						tokensToAdd.Add( t );
				}

				// find out what tokens need to be removed
				List<PropertyToken> tokensToRemove = new List<PropertyToken>();
				foreach( var t in m_objectNodes )
				{
					if( !tokensFlatList.Contains( t.Key ) )
						tokensToRemove.Add( t.Key );
				}

				// get rid of all the tokens that shouldn't exist
				foreach( PropertyToken t in tokensToRemove )
				{
					BasicNode n = m_objectNodes[ t ];
					n.HideEditControls();
					n.Remove();
					m_objectNodes.Remove( t );
				}

				// create all of the nodes that should exist
				foreach( PropertyToken t in tokensToAdd )
				{
					if( t.m_iElementIndex >= 0 )
					{
						// Array element
						ArrayElementNode newNode = new ArrayElementNode( this, t, "[" + t.m_iElementIndex.ToString() + "]" );
						m_objectNodes[ t ] = newNode;
					}
					else if( t.m_iElementIndex == -2 )
					{
						// Array adder node
						ArrayNode newNode = new ArrayNode( this, t, "" );
						m_objectNodes[ t ] = newNode;
					}
					else
					{
						// Normal
						BasicNode newNode = new BasicNode( this, t, t.m_property.Name );
						m_objectNodes[ t ] = newNode;
					}
				}

				// make sure children array is up to date
				//foreach( 

				// Nodes are all initialized, now need to hook their hierarchy up
				foreach( var pair in m_objectNodes )
				{
					PropertyToken t = pair.Key;
					BasicNode n = pair.Value;

					// root level entries go straight in
					if( t.m_parent == null )
					{
						if( n.Parent != null )
							n.Remove();
					
						if( !m_ctlTreeView.Nodes.Contains( n ) )
							m_ctlTreeView.Nodes.Add( n );
					}
					else
					{
						BasicNode parentNode = m_objectNodes[ t.m_parent ];
						if( n.Parent == null )
						{
							parentNode.Nodes.Add( n );
						}
						else if( n.Parent != parentNode )
						{
							n.Remove();
							parentNode.Nodes.Add( n );
						}
					}
				}

				// final pass to make sure that the Array adder node is always last - HACK
				// TODO: implement a sort order
				foreach( var pair in m_objectNodes )
				{
					PropertyToken t = pair.Key;
					BasicNode n = pair.Value;
					if( t.m_iElementIndex == -2 )
					{
						n.Remove();
						m_objectNodes[ t.m_parent ].Nodes.Add( n );
					}
				}
				
				RefreshExtras();
			}
		}
		//--------------------------------------------------------------------

		public Object GetItem()
		{
			return m_link;
		}
		//--------------------------------------------------------------------

		private void RefreshExtras()
		{
			TreeNode oldTop = m_ctlTreeView.TopNode;
			int iOldScroll = m_usableNodes.IndexOf( oldTop as BasicNode );
			m_usableNodes.Clear();

			int iMaxWidth = 0;
			foreach( TreeNode n in m_ctlTreeView.Nodes )
				RefreshNodeExtras( n, true, ref iMaxWidth );

			m_iMaxTreeWidth = iMaxWidth;
			FixLayout();
		}
		//--------------------------------------------------------------------

		private void RefreshNodeExtras( TreeNode n, bool bParentExpanded, ref int iMaxWidth )
		{
			BasicNode myNode = n as BasicNode;
			if( bParentExpanded )
			{
				myNode.ShowEditControls();
				m_usableNodes.Add( myNode );
				if( myNode.Bounds.Right > iMaxWidth )
					iMaxWidth = myNode.Bounds.Right;
			}
			else
				myNode.HideEditControls();

			foreach( TreeNode child in myNode.Nodes )
				RefreshNodeExtras( child, bParentExpanded && myNode.IsExpanded, ref iMaxWidth );
		}

		private void BeforeTreeChange( object sender, TreeViewCancelEventArgs e )
		{
			RefreshExtras();
		}

		private void AfterTreeChange( object sender, TreeViewEventArgs e )
		{
			RefreshExtras();
		}

		private void MegaPropertyGrid_Paint( object sender, PaintEventArgs e )
		{
		//	FixLayout();
		}

		private void DrawNode( object sender, DrawTreeNodeEventArgs e )
		{
			//e.
			e.DrawDefault = true;
		/*	if( e.Node is BasicNode )
			{
				( e.Node as BasicNode ).PositionEditControls();
			}*/
		}

		private void MegaPropertyGrid_Load( object sender, EventArgs e )
		{
			m_iMaxTreeWidth = m_ctlTreeView.Width / 2;
			FixLayout();
		}

		private void FixLayout()
		{
			// now we know how many nodes should be visible, so resize to suit
			m_ctlTreeView.ItemHeight = 19;
			m_ctlTreeView.Width = Width;
			int iMaxHeight = m_ctlTreeView.ItemHeight * m_usableNodes.Count();
			if( iMaxHeight > Height )
				m_ctlTreeView.Height = iMaxHeight;
			else
				m_ctlTreeView.Height = Height;

			foreach( BasicNode n in m_usableNodes )
				n.PositionEditControls();
		}

		private void RefreshClicked( object sender, EventArgs e )
		{
			Dirty();
		}

		private delegate void EmptyDelegate();
		public void Dirty()
		{
			this.BeginInvoke( new EmptyDelegate( RefreshItem ) );
		}

		public void RefreshItem()
		{
			SetItem( GetItem() );
		}

	}
}
