//******************************************************************************************************
//  MainWindowViewModel.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Forms;
using StartingFromScratch.Commands;
using SynchrophasorAnalytics.Networks;
using SynchrophasorAnalytics.Modeling;

namespace StartingFromScratch.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private List<MenuItemViewModel> m_menuBarItems;
        private NetworkTreeViewModel m_networkTreeViewModel;
        private RecordDetailViewModel m_recordDetail;
        private Network m_network;
        private RelayCommand m_openFileCommand;
        private RelayCommand m_saveFileCommand;
        private RelayCommand m_changeSelectedElementCommand;
        private RelayCommand m_viewDetailCommand;
        private RelayCommand m_refreshNetworkTreeCommand;
        private string m_actionStatus;

        public NetworkTreeViewModel NetworkTree
        {
            get
            {
                return m_networkTreeViewModel;
            }
            set
            {
                m_networkTreeViewModel = value;
            }
        }

        public RecordDetailViewModel RecordDetail
        {
            get
            {
                return m_recordDetail;
            }
            set
            {
                m_recordDetail = value;
            }
        }

        public ObservableCollection<MenuItemViewModel> MenuBarItems
        {
            get
            {
                return new ObservableCollection<MenuItemViewModel>(m_menuBarItems);
            }
        }

        public string ActionStatus
        {
            get
            {
                return m_actionStatus;
            }
            set
            {
                m_actionStatus = value;
            }
        }

        public ICommand OpenFileCommand
        {
            get 
            {
                if (m_openFileCommand == null)
                {
                    m_openFileCommand = new RelayCommand(param => this.OpenFile(), param => true);
                }
                return m_openFileCommand;
            }
        }

        public ICommand SaveFileCommand
        {
            get
            {
                if (m_saveFileCommand == null)
                {
                    m_saveFileCommand = new RelayCommand(param => this.SaveFile(), param => true);
                }
                return m_saveFileCommand;
            }
        }

        public ICommand ChangeSelectedElementCommand
        {
            get
            {
                if (m_changeSelectedElementCommand == null)
                {
                    m_changeSelectedElementCommand = new RelayCommand(param => this.ChangeSelectedElement(), param => true);
                }
                return m_changeSelectedElementCommand;
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

        public ICommand UpdateTreeViewCommand
        {
            get
            {
                if (m_refreshNetworkTreeCommand == null)
                {
                    m_refreshNetworkTreeCommand = new RelayCommand(param => this.RefreshNetworkTree(), param => true);
                }
                return m_refreshNetworkTreeCommand;
            }
        }

        private void RefreshNetworkTree()
        {
            m_networkTreeViewModel.RefreshNetworkTree();
        }

        public bool CanOpenFile
        {
            get
            {
                return true;
            }
        }

        public MainWindowViewModel()
        {
            InitializeMenuBar();
            InitializeNetworkTree();
            InitializeRecordDetail();
            
        }

        private void InitializeMenuBar()
        {
            m_menuBarItems = new List<MenuItemViewModel>();

            MenuItemViewModel fileMenuItem = new MenuItemViewModel("File", null);
            MenuItemViewModel openMenuItem = new MenuItemViewModel("Open", null);
            openMenuItem.AddMenuItem(new MenuItemViewModel("Xml Network Model", OpenFileCommand));
            fileMenuItem.AddMenuItem(openMenuItem);
            fileMenuItem.AddMenuItem(new MenuItemViewModel("Save", SaveFileCommand));
            fileMenuItem.AddMenuItem(new MenuItemViewModel("Exit", null));
            //MenuItemViewModel utilitiesMenuItem = new MenuItemViewModel("Utilities", null);
            //MenuItemViewModel testCaseGenerator = new MenuItemViewModel("Generate Measurement Test Case", null);

            //utilitiesMenuItem.AddMenuItem(testCaseGenerator);

            m_menuBarItems.Add(fileMenuItem);
        }

        private void InitializeNetworkTree()
        {
            m_network = new Network();
            m_network.Initialize();
            m_network.Model = new NetworkModel();
            m_networkTreeViewModel = new NetworkTreeViewModel(this, m_network);
        }

        private void InitializeRecordDetail()
        {
            m_recordDetail = new RecordDetailViewModel(this);
        }

        #region [ Commands ]

        private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.DefaultExt = ".xml";
            openFileDialog.Filter = "Network Model (.xml)|*.xml";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    m_network = Network.DeserializeFromXml(openFileDialog.FileName);
                    
                    m_networkTreeViewModel.Network = m_network;
                }
                catch (Exception exception)
                {
                    if (exception != null)
                    {
                        System.Windows.MessageBox.Show(exception.ToString(), "Failed to load selected file.");
                    }
                }
            }
        }


        private void ChangeSelectedElement()
        {
            System.Windows.MessageBox.Show("Selection Changed");
        }

        private void ViewDetail()
        {
            m_recordDetail.ClearRecordDetailView();
            m_recordDetail.AddViewModel(m_networkTreeViewModel.SelectedElement.Value);
        }

        private void SaveFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.Filter = "Network Model (.xml)|*.xml";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    m_network.Initialize();
                    m_network.SerializeToXml(saveFileDialog.FileName);
                }
                catch (Exception exception)
                {
                    if (exception != null)
                    {
                        System.Windows.MessageBox.Show(exception.ToString(), "Failed to save xml file.");
                    }
                }
            }
        }

        #endregion
    }
}
