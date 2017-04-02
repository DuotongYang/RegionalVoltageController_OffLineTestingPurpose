//******************************************************************************************************
//  NetworkTreeViewModel.cs
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
//
//******************************************************************************************************
using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using StartingFromScratch.Commands;
using SynchrophasorAnalytics.Networks;
using SynchrophasorAnalytics.Modeling;

namespace StartingFromScratch.ViewModels
{
    public class NetworkTreeViewModel : ViewModelBase
    {

        #region [ Private Members ]

        private MainWindowViewModel m_mainWindow;
        private Network m_network;
        private NetworkElementViewModel m_root;
        private ObservableCollection<NetworkElementViewModel> m_firstGeneration;
        private RelayCommand m_viewDetailCommand;
        private NetworkElementViewModel m_selectedElement;
        
        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns a read-only collection containing the first person 
        /// in the family tree, to which the TreeView can bind.
        /// </summary>
        public ObservableCollection<NetworkElementViewModel> FirstGeneration
        {
            get 
            { 
                return m_firstGeneration; 
            }
        }

        public Network Network
        {
            get
            {
                return m_network;
            }
            set
            {
                m_network = value;
                InitializeNetworkTree(value);
            }
        }

        public NetworkElementViewModel SelectedElement
        {
            get
            {
                return m_selectedElement;
            }
            set
            {
                m_selectedElement = value;
            }
        }

        public ObservableCollection<MenuItem> ContextMenuItems
        {
            get
            {
                List<MenuItem> contextMenuItems = new List<MenuItem>();
                contextMenuItems.Add(new MenuItem() { Header = "Item 1" });
                contextMenuItems.Add(new MenuItem() { Header = "Item 2" });
                return new ObservableCollection<MenuItem>(contextMenuItems);
            }
        }
        
        public ICommand ViewDetailCommand
        {
            get
            {
                if (m_viewDetailCommand == null)
                {
                    m_viewDetailCommand = new RelayCommand(param => this.ViewDetail(), param => true);
                }
                return m_viewDetailCommand;
            }
        }

        public ViewModelBase MainWindow
        {
            get
            {
                return m_mainWindow;
            }
            set
            {
                if (value is MainWindowViewModel)
                {
                    m_mainWindow = value as MainWindowViewModel;
                }
            }
        }

        

        #endregion

        #region Constructor

        public NetworkTreeViewModel(MainWindowViewModel mainWindow)
            :this(mainWindow, null)
        {
        }

        public NetworkTreeViewModel(MainWindowViewModel mainWindow, Network network)
        {
            m_mainWindow = mainWindow;
            if (network != null)
            {
                InitializeNetworkTree(network);
            }
            else
            {
                m_network = new Network();
                InitializeNetworkTree(m_network);
            }
        }

        private void InitializeNetworkTree(Network network)
        {
            network.Initialize();

            m_root = new NetworkElementViewModel(this, new NetworkElement(network));
            
            m_firstGeneration = new ObservableCollection<NetworkElementViewModel>(new NetworkElementViewModel[] { m_root });
            OnPropertyChanged("FirstGeneration");
        }
        
        #endregion // Constructor

        internal void HandleRightMouseButtonClick()
        {
            if (SelectedElement != null)
            {
                MessageBox.Show(SelectedElement.Name);
            }
        }
        
        private void ViewDetail()
        {
            m_mainWindow.ViewDetailCommand.Execute(null);
        }

        public void RefreshNetworkTree()
        {
            InitializeNetworkTree(m_network);
        }
    }
}
