//******************************************************************************************************
//  NetworkElementViewModel.cs
//
//  Copyright © 2014, Kevin D. Jones.  All Rights Reserved.
//
//  This file is licensed to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/01/2014 - Kevin D. Jones
//       Generated original version of source code.
//  05/12/2014 - Kevin D. Jones
//       Added functionality for Shunt branches and added some 'region' markup
//
//******************************************************************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using StartingFromScratch.Interfaces;
using StartingFromScratch.Commands;
using SynchrophasorAnalytics.Networks;
using SynchrophasorAnalytics.Modeling;
using SynchrophasorAnalytics.Measurements;

namespace StartingFromScratch.ViewModels
{
    public class NetworkElementViewModel : ViewModelBase, IContextMenu
    {
        #region [ Private Fields ]

        private NetworkTreeViewModel m_networkTree;
        private NetworkElementViewModel m_parent;
        private NetworkElement m_networkElement;

        private RelayCommand m_createCommand;
        private RelayCommand m_createSubstationCommand;
        private RelayCommand m_createTransmissionLineCommand;
        private RelayCommand m_createNodeCommand;
        private RelayCommand m_createShuntCommand;
        private RelayCommand m_createSwitchCommand;
        private RelayCommand m_createCircuitBreakerCommand;
        private RelayCommand m_createTransformerCommand;
        private RelayCommand m_createLineSegmentCommand;
        private RelayCommand m_viewDetailCommand;
        private RelayCommand m_viewXmlSourceCommand;
        private RelayCommand m_refreshCommand;
        private RelayCommand m_onMouseDoubleClickCommand;
        private RelayCommand m_deleteCommand;

        private bool m_isExpanded;
        private bool m_isSelected;

        #endregion

        #region [ Properties ]

        public ICommand OnMouseDoubleClick
        {
            get
            {
                if (m_onMouseDoubleClickCommand == null)
                {
                    m_onMouseDoubleClickCommand = new RelayCommand(param => this.ViewDetail(), param => true);
                }
                return m_onMouseDoubleClickCommand;
            }
        }

        public NetworkElement Value
        {
            get
            {
                return m_networkElement;
            }
            set
            {
                m_networkElement = value;
            }
        }

        public NetworkTreeViewModel NetworkTree
        {
            get
            {
                return m_networkTree;
            }
        }

        public ReadOnlyCollection<NetworkElementViewModel> Children
        {
            get 
            {
                return GetNetworkElementViewModelChildren();
            }
        }
        
        public NetworkElementViewModel Parent
        {
            get
            {
                return m_parent;
            }
        }

        public ObservableCollection<MenuItem> ContextMenuItems
        {
            get
            {
                return GetNetworkElementContextMenuItems();
            }
        }

        public string Name
        {
            get
            {
                return m_networkElement.Name;
            }
        }

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return m_isExpanded; }
            set
            {
                if (value != m_isExpanded)
                {
                    m_isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (m_isExpanded && m_parent != null)
                    Parent.IsExpanded = true;
            }
        }

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get 
            { 
                return m_isSelected; 
            }
            set
            {
                if (value != m_isSelected)
                {
                    m_isSelected = value;
                    this.OnPropertyChanged("IsSelected");

                    if (m_isSelected)
                    {
                        m_networkTree.SelectedElement = this;
                    }
                }
            }
        }

        #endregion    

        #region [ Constructors ]

        public NetworkElementViewModel(NetworkTreeViewModel networkTree, NetworkElement networkElement)
        {
            m_networkTree = networkTree;
            m_networkElement = networkElement;
            m_parent = null;
        }

        public NetworkElementViewModel(NetworkElement networkElement, NetworkElementViewModel parent)
        {
            m_networkTree = parent.NetworkTree;
            m_parent = parent;
            m_networkElement = networkElement;
        }

        #endregion

        #region [ Private Methods ]

        private ReadOnlyCollection<NetworkElementViewModel> GetNetworkElementViewModelChildren()
        {
            if (m_networkElement != null && m_networkElement.Children != null)
            {
                List<NetworkElementViewModel> children = new List<NetworkElementViewModel>();
                foreach (NetworkElement child in m_networkElement.Children)
                {
                    children.Add(new NetworkElementViewModel(child, this));
                }
                return new ReadOnlyCollection<NetworkElementViewModel>(children);
            }
            else
            {
                return null;
            }
        }

        private ObservableCollection<MenuItem> GetNetworkElementContextMenuItems()
        {
            InitializeMenuItemCommands();

            MenuItem viewMenuItem = new MenuItem() { Header = "View" };
            viewMenuItem.Items.Add(new MenuItem() { Header = "Detail", Command = m_viewDetailCommand });
            viewMenuItem.Items.Add(new MenuItem() { Header = "Xml Source", Command = m_viewXmlSourceCommand });
            MenuItem refreshMenuItem = new MenuItem() { Header = "Refresh", Command = m_refreshCommand };
            MenuItem deleteMenuItem = new MenuItem() { Header = "Delete", Command = m_deleteCommand };

            if (m_networkElement.Element is NetworkModel ||
                m_networkElement.Element is List<VoltageLevel> ||
                m_networkElement.Element is List<BreakerStatus> ||
                m_networkElement.Element is List<TapConfiguration> ||
                m_networkElement.Element is List<Substation> ||
                m_networkElement.Element is List<TransmissionLine> ||
                m_networkElement.Element is List<Node> ||
                m_networkElement.Element is List<ShuntCompensator> ||
                m_networkElement.Element is List<Switch> ||
                m_networkElement.Element is List<CircuitBreaker> ||
                m_networkElement.Element is List<Transformer> ||
                m_networkElement.Element is List<SeriesCompensator> ||
                m_networkElement.Element is List<LineSegment>)
            {
                MenuItem addMenuItem = new MenuItem() { Header = "Add", Command = m_createCommand };
                return new ObservableCollection<MenuItem>(new MenuItem[] { addMenuItem, viewMenuItem, refreshMenuItem });
            }
            else if (m_networkElement.Element is Company)
            {
                MenuItem addMenuItem = new MenuItem() { Header = "Add", Command = m_createCommand };
                return new ObservableCollection<MenuItem>(new MenuItem[] { addMenuItem, viewMenuItem, refreshMenuItem, deleteMenuItem });
            }
            else if (m_networkElement.Element is Division)
            {
                MenuItem addMenuItem = new MenuItem() { Header = "Add" };
                addMenuItem.Items.Add(new MenuItem() { Header = "Substation", Command = m_createSubstationCommand });
                addMenuItem.Items.Add(new MenuItem() { Header = "Transmission Line", Command = m_createTransmissionLineCommand });
                return new ObservableCollection<MenuItem>(new MenuItem[] { addMenuItem, viewMenuItem, refreshMenuItem, deleteMenuItem });
            }
            else if (m_networkElement.Element is Substation)
            {
                MenuItem addMenuItem = new MenuItem() { Header = "Add" };
                addMenuItem.Items.Add(new MenuItem() { Header = "Node", Command = m_createNodeCommand });
                addMenuItem.Items.Add(new MenuItem() { Header = "Shunt", Command = m_createShuntCommand });
                addMenuItem.Items.Add(new MenuItem() { Header = "Circuit Breaker", Command = m_createCircuitBreakerCommand });
                addMenuItem.Items.Add(new MenuItem() { Header = "Switch", Command = m_createSwitchCommand });
                addMenuItem.Items.Add(new MenuItem() { Header = "Transformer", Command = m_createTransformerCommand });
                return new ObservableCollection<MenuItem>(new MenuItem[] { addMenuItem, viewMenuItem, refreshMenuItem, deleteMenuItem });
            }
            else if (m_networkElement.Element is TransmissionLine)
            {
                MenuItem addMenuItem = new MenuItem() { Header = "Add" };
                addMenuItem.Items.Add(new MenuItem() { Header = "LineSegment", Command = m_createLineSegmentCommand });
                return new ObservableCollection<MenuItem>(new MenuItem[] { addMenuItem, viewMenuItem, refreshMenuItem, deleteMenuItem });
            }
            else if (m_networkElement.Element is List<StatusWord>)
            {
                MenuItem addMenuItem = new MenuItem() { Header = "Add" };
                addMenuItem.Items.Add(new MenuItem() { Header = "New Status Word", Command = m_createCommand });
                return new ObservableCollection<MenuItem>(new MenuItem[] { addMenuItem, viewMenuItem, refreshMenuItem });
            }
            else if (m_networkElement.Element is List<TapConfiguration>)
            {
                MenuItem addMenuItem = new MenuItem() { Header = "Add" };
                return new ObservableCollection<MenuItem>(new MenuItem[] { addMenuItem, viewMenuItem, refreshMenuItem});
            }
            else
            {
                return new ObservableCollection<MenuItem>(new MenuItem[] { viewMenuItem, refreshMenuItem, deleteMenuItem });
            }
        }

        #endregion

        #region [ Commands ]

        private void InitializeMenuItemCommands()
        {
            InitializeCreateCommands();
            m_viewDetailCommand = new RelayCommand(param => this.ViewDetail(), param => true);
            m_viewXmlSourceCommand = new RelayCommand(param => this.ViewXml(), param => true);
            m_refreshCommand = new RelayCommand(param => this.RefreshNetworkTree(), param => true);
            m_deleteCommand = new RelayCommand(param => this.Delete(), param => true);
        }
        
        private void InitializeCreateCommands()
        {
            m_createCommand = new RelayCommand(param => this.CreateNew(), param => true);
            m_createSubstationCommand = new RelayCommand(param => this.CreateNewSubstation(), param => true);
            m_createTransmissionLineCommand = new RelayCommand(param => this.CreateNewTransmissionLine(), param => true);
            m_createNodeCommand = new RelayCommand(param => this.CreateNewNode(), param => true);
            m_createShuntCommand = new RelayCommand(param => this.CreateNewShunt(), param => true);
            m_createCircuitBreakerCommand = new RelayCommand(param => this.CreateNewCircuitBreaker(), param => true);
            m_createSwitchCommand = new RelayCommand(param => this.CreateNewSwitch(), param => true);
            m_createLineSegmentCommand = new RelayCommand(param => this.CreateNewLineSegment(), param => true);
            m_createTransformerCommand = new RelayCommand(param => this.CreateNewTransformer(), param => true);
        }

        private void Delete()
        {
            NetworkElementDestroyer.DestroyNetworkElement(this);
            Parent.OnPropertyChanged("Children");
        }

        private void CreateNew()
        {
            NetworkElementFactory.CreateNewChildElement(this);
            OnPropertyChanged("Children");
        }

        private void CreateNewSubstation()
        {
            NetworkElementFactory.CreateNewSubstation(m_networkElement.Element as Division);
            OnPropertyChanged("Children");
        }

        private void CreateNewTransmissionLine()
        {
            NetworkElementFactory.CreateNewTransmissionLine(m_networkElement.Element as Division);
            OnPropertyChanged("Children");
        }

        private void CreateNewNode()
        {
            if (m_networkElement.Element is Substation)
            {
                NetworkElementFactory.CreateNewNode(m_networkElement.Element as Substation);
            }
            else if (m_networkElement.Element is TransmissionLine)
            {
                NetworkElementFactory.CreateNewNode(m_networkElement.Element as TransmissionLine);
            }
            OnPropertyChanged("Children");
        }

        private void CreateNewShunt()
        {
            NetworkElementFactory.CreateNewShunt(m_networkElement.Element as Substation);
        }

        private void CreateNewSwitch()
        {
            if (m_networkElement.Element is Substation)
            {
                NetworkElementFactory.CreateNewSwitch(m_networkElement.Element as Substation);
            }
            else if (m_networkElement.Element is TransmissionLine)
            {
                NetworkElementFactory.CreateNewSwitch(m_networkElement.Element as TransmissionLine);
            }
            OnPropertyChanged("Children");
        }

        private void CreateNewCircuitBreaker()
        {
            NetworkElementFactory.CreateNewCircuitBreaker(m_networkElement.Element as Substation);
            OnPropertyChanged("Children");
        }

        private void CreateNewLineSegment()
        {
            NetworkElementFactory.CreateNewLineSegment(m_networkElement.Element as TransmissionLine);
            OnPropertyChanged("Children");
        }

        private void CreateNewTransformer()
        {
            NetworkElementFactory.CreateNewTransformer(m_networkElement.Element as Substation);
            OnPropertyChanged("Children");
        }

        private void ViewDetail()
        {
            m_networkTree.ViewDetailCommand.Execute(null);
        }

        private void ViewXml()
        {
            MessageBox.Show(m_networkElement.AsXml());
        }

        private void RefreshNetworkTree()
        {
            OnPropertyChanged("Children");
        }

        #endregion
    }
}
