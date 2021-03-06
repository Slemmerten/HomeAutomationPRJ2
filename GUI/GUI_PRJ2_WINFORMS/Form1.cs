﻿using HomeAutomationLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Forms;

namespace GUI_PRJ2_WINFORMS
{
    public partial class Form1 : Form
    {
        private List<Apparat> availableApparats = new List<Apparat>(); //List used to store the available apparats of the app
        private SerialCom serialCom; //Serial Com class used for UART communication
        private int currentApparatPort = 0; //The port of the currently chosen apparat
        private Func currentApparatFunc = Func.OnOff; //The functionality of the currently chosen apparat
        private string log; //The string used to store the log data on

        public Form1()
        {
            InitializeComponent();

            //Sets up dummy data
            SetupData();

            //Sets up list view
            SetupListView(listView1);

            //Populate the Combobox with SerialPorts on the System
            comboBox_available_serialPorts.Items.AddRange(SerialPort.GetPortNames());

            //Setup serialCom
            serialCom = new SerialCom(serialPort1);

            // Displaying System Information 

            string SystemInformation;//Used for Storing System Information 

            //Log the system information
            SystemInformation = " Machine Name = " + System.Environment.MachineName;                                         // Get the Machine Name 
            SystemInformation = SystemInformation + Environment.NewLine + " OS Version = " + System.Environment.OSVersion;    // Get the OS version
            SystemInformation = SystemInformation + Environment.NewLine + " Processor Cores = " + Environment.ProcessorCount + Environment.NewLine; // Get the Number of Processors on the System

            TextBox_System_Log.AppendText(SystemInformation); //Display into the Log Text Box
        }
        
        /// <summary>
        /// Function for setup of listview with use of helper class<see cref="ListViewExtender"/>
        /// <paramref name="listViewToSetup">The listview to setup</paramref>
        /// </summary>
        private void SetupListView(ListView listViewToSetup)
        {
            listViewToSetup.FullRowSelect = true;
            //Create extender
            ListViewExtender extender = new ListViewExtender(listViewToSetup);
            // extend 2nd column
            ListViewButtonColumn buttonAction = new ListViewButtonColumn(1);
            //Add action for button
            buttonAction.Click += Apparat_Click;
            buttonAction.FixedWidth = true;
            //Add column with dummy data
            extender.AddColumn(buttonAction);
            //Update listview
            UpdateListView(listViewToSetup);
        }

        /// <summary>
        /// Function for updating a listview
        /// </summary>
        /// <param name="listView2Update">The list view to update</param>
        private void UpdateListView(ListView listView2Update)
        {
            //Delete existing listview items
            if (availableApparats.Count != 0)
            {
                for (int i = 0; i < listView2Update.Items.Count; i++)
                {
                    listView2Update.Items[i].Remove();
                    i--;
                }
            }

            //Add new items
            for (int i = 0; i < availableApparats.Count; i++)
            {
                ListViewItem item = listView2Update.Items.Add(availableApparats[i].Name);
                item.SubItems.Add(availableApparats[i].Port.ToString());
            }

        }

        /// <summary>
        /// Function to check if the port is on
        /// </summary>
        /// <param name="port">The port to check</param>
        /// <returns></returns>
        private bool IsPortOn(int port)
        {
            return availableApparats.Find(item => item.Port == port).OnOff;
        }

        /// <summary>
        /// Function for changing page
        /// </summary>
        /// <param name="applicationPage"></param>
        private void ChangePage(ApplicationPage applicationPage)
        {
            switch (applicationPage)
            {
                case ApplicationPage.ApparatMenu:
                    //Change page to ApparatMenu
                    ApparatMenu.Enabled = true;
                    AddMenu.Enabled = false;
                    Settings.Enabled = false;
                    mainView.SelectTab(ApparatMenu);
                    break;
                case ApplicationPage.ApparatSettings:
                    //Change page to ApparatSettings
                    ApparatMenu.Enabled = false;
                    AddMenu.Enabled = false;
                    Settings.Enabled = true;
                    mainView.SelectTab(Settings);
                    break;
                case ApplicationPage.AddApparat:
                    //Change page to AddMenu
                    ApparatMenu.Enabled = false;
                    Settings.Enabled = false;
                    AddMenu.Enabled = true;
                    mainView.SelectTab(AddMenu);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Private helper method for list view button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Apparat_Click(object sender, ListViewColumnMouseEventArgs e)
        {
            //Check if com is selected
            if (comboBox_available_serialPorts.SelectedIndex == -1)
            {
                MessageBox.Show("Please Select COM-port.");
                return;
            }
            //Check if baud is selected
            else if(comboBox_baudRate.SelectedIndex == -1)
            {
                MessageBox.Show("Please Select Baudrate.");
                return;
            }
            AppAction current = new AppAction(availableApparats[e.Item.Index]);
            if (current.SelectedOnOff)
            {
                //Enable On Off functionality
                onOffButton.Visible = true;
                onOffButton.Enabled = true;
                //Sets the text of the on of button
                onOffButton.Text = (IsPortOn(availableApparats[e.Item.Index].Port) ? "Turn Off" : "Turn On");
            }
            else
            {
                //Disable On Off functionality
                onOffButton.Visible = false;
                onOffButton.Enabled = false;
            }
            if (current.SelectedDimmer)
            {
                //Enable dimmer functionality
                dimmerScroll.Visible = true;
                dimmerScroll.Enabled = true;
                dimmerText.Visible = true;
                //Sets the dimmerscroll value
                dimmerScroll.Value = current.SelectedDimmerValue;
            }
            else
            {
                //Disable dimmer functionality
                dimmerScroll.Visible = false;
                dimmerScroll.Enabled = false;
                dimmerText.Visible = false;
            }
            //Set the current apparat port
            currentApparatPort = availableApparats[e.Item.Index].Port;
            //Set the current apparat functionality
            currentApparatFunc = availableApparats[e.Item.Index].Functionality;
            //Set the label for current apparat
            currentApparatLabel.Text = availableApparats[e.Item.Index].Name;
            //Change page to Apparat settings
            ChangePage(ApplicationPage.ApparatSettings);
        }

        /// <summary>
        /// Sets up dummy data
        /// </summary>
        private void SetupData()
        {
            //Add default object as dummy data
            availableApparats.Add(new Apparat());
            //Add Scrollable dummy apparat
            availableApparats.Add(new Apparat("ScrollableDummy", 1, Func.Dimmer | Func.OnOff));
        }
        
        /// <summary>
        /// Event to go to Add Apparat page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddApparat_Click(object sender, EventArgs e)
        {
            //Change page to AddMenu
            ChangePage(ApplicationPage.AddApparat);
        }

        /// <summary>
        /// Event to go back to apparat menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, EventArgs e)
        {
            //Change page to ApparatMenu
            ChangePage(ApplicationPage.ApparatMenu);
        }

        /// <summary>
        /// Event to add the new apparat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, EventArgs e)
        {
            //Create new apparat
            Apparat apparatToAdd = new Apparat();
            //Set name to the text of apparatNameTextbox
            apparatToAdd.Name = (!string.IsNullOrEmpty(apparatNameTextbox.Text) ? apparatNameTextbox.Text : "Unknown");
            
            //Set functionality of the apparat
            foreach (object indexChecked in functionalityCheckBox.CheckedIndices)
            {
                apparatToAdd.Functionality |= (Func)((int)indexChecked + 1);
            }

            //Set Port to the port chosen
            apparatToAdd.Port = portComboBox.SelectedIndex;

            //If the port chosen already exist
            if (availableApparats.Exists(x => x.Port == portComboBox.SelectedIndex))
            {
                //Replace the apparat
                int index = availableApparats.FindIndex(x => x.Port == portComboBox.SelectedIndex);
                availableApparats[index] = apparatToAdd;
            }
            else
            {
                //Else add new apparat
                availableApparats.Add(apparatToAdd);
            }
            //update listview
            UpdateListView(listView1);
            //Write to log
            log = apparatToAdd.Name + " on port " + apparatToAdd.Port.ToString() + " added to list" + Environment.NewLine;
            TextBox_System_Log.AppendText(log);
            //change page to apparat menu
            ChangePage(ApplicationPage.ApparatMenu);
        }

        /// <summary>
        /// Event for when the form is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            //Disable other pages
            AddMenu.Enabled = false;
            Settings.Enabled = false;
            //Add necessary columns to the listview for the listview extender to work
            listView1.Columns.Add("Name", 140);
            listView1.Columns.Add("Port", 60);
            //Set the selected index of the port
            portComboBox.SelectedIndex = 0;
            //Check checkbox OnOff 
            functionalityCheckBox.SetItemChecked(0, true);
            //Hide tab header
            mainView.Appearance = TabAppearance.FlatButtons;
            mainView.ItemSize = new Size(0, 1);
            mainView.SizeMode = TabSizeMode.Fixed;
            
        }

        /// <summary>
        /// Event for when the OnOff button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOffButton_Click(object sender, EventArgs e)
        {
            //Check if dimmer is enabled
            if ((currentApparatFunc & Func.Dimmer) == Func.Dimmer)
            {
                //If port is On turn it Off
                if (IsPortOn(currentApparatPort))
                {
                    log = "Send: " + serialCom.OnOff(currentApparatPort, IsPortOn(currentApparatPort)) + Environment.NewLine;
                    TextBox_System_Log1.AppendText(log);
                    //Invert on/off
                    availableApparats.Find(item => item.Port == currentApparatPort).OnOff = false;
                }
                //else dimm instead of setting max
                else
                {
                    //Also write to log
                    log = "Send: " + serialCom.Dimm(currentApparatPort, dimmerScroll.Value) + Environment.NewLine;
                    TextBox_System_Log1.AppendText(log);
                    //Invert on/off
                    availableApparats.Find(item => item.Port == currentApparatPort).OnOff = true;
                }
                //Change text of onOffButton
                onOffButton.Text = (IsPortOn(currentApparatPort) ? "Turn Off" : "Turn On");
            }
            else
            {
                //Turn the light on/off and write to log
                log = "Send: " + serialCom.OnOff(currentApparatPort, IsPortOn(currentApparatPort)) + Environment.NewLine;
                TextBox_System_Log1.AppendText(log);
                //Invert on/off
                availableApparats.Find(item => item.Port == currentApparatPort).OnOff ^= true;
                //Change text of onOffButton
                onOffButton.Text = (IsPortOn(currentApparatPort) ? "Turn Off" : "Turn On");
            }
        }

        /// <summary>
        /// Event for when the dimmer is scrolled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dimmer_Scroll(object sender, EventArgs e)
        {
            //Set the value of the dimmer
            availableApparats.Find(item => item.Port == currentApparatPort).DimmerValue = dimmerScroll.Value;
            //Dimm the light
            log = "Send: " + serialCom.Dimm(currentApparatPort, dimmerScroll.Value) + Environment.NewLine;
            TextBox_System_Log1.AppendText(log);
        }

        /// <summary>
        /// Event for when the delete button is presssed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            //Foreach selected item in listview1: Delete
            foreach(ListViewItem selected in listView1.SelectedItems)
            {
                //Write it to log
                log = availableApparats[selected.Index].Name + " removed." + Environment.NewLine;
                TextBox_System_Log.AppendText(log);
                //Remove it
                availableApparats.RemoveAt(selected.Index);
                selected.Remove();
            }
        }

        /// <summary>
        /// Event for when the COM is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_available_serialPorts_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // Store the Selected COM port
            log = comboBox_available_serialPorts.SelectedItem.ToString() + " Selected" + Environment.NewLine;
            // Display the Selected COM port in the Log Text Box
            TextBox_System_Log.AppendText(log);
            //Sets the portName
            serialPort1.PortName = comboBox_available_serialPorts.SelectedItem.ToString();
        }

        /// <summary>
        /// Event for when the baud rate is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_baudRate_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // Store the Selected Baud rate
            log = "Baudrate of " + comboBox_baudRate.SelectedItem.ToString() + " Selected" + Environment.NewLine;
            // Display the Selected COM port in the Log Text Box
            TextBox_System_Log.AppendText(log);
            //Sets the baudrate of serialPort1
            serialPort1.BaudRate = Convert.ToInt32(comboBox_baudRate.SelectedItem.ToString());
        }
        /// <summary>
        /// Event for when the user lets go off the dimmer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DimmerScroll_MouseUp(object sender, MouseEventArgs e)
        {
            //Set the value of the dimmer
            availableApparats.Find(item => item.Port == currentApparatPort).DimmerValue = dimmerScroll.Value;
            if (IsPortOn(currentApparatPort))
            {
                //Dimm the light
                log = "Send: " + serialCom.Dimm(currentApparatPort, dimmerScroll.Value) + Environment.NewLine;
                TextBox_System_Log1.AppendText(log);
            }
        }
    }
}
