using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;
using FTN.ESI.SIMES.CIM.CIMAdapter;
using FTN.ServiceContracts;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    enum Profiles
    {
        ProjekatMPPM
    }
    public partial class MainWindow : Window
    {
        // Polja za prvi tab
        private CIMAdapter adapter = new CIMAdapter();
        private Delta nmsDelta = null;

        // Kolekcija za prikaz GID-ova u drugom tabu
        private ObservableCollection<GidDisplayItem> loadedGidsForDisplay = new ObservableCollection<GidDisplayItem>();
        private ObservableCollection<GidDisplayItem> extentGidsForDisplay = new ObservableCollection<GidDisplayItem>();
        private ObservableCollection<GidDisplayItem> relatedGidsForDisplay = new ObservableCollection<GidDisplayItem>();

        private ObservableCollection<GidDisplayItem> allEntitiesForCombo = new ObservableCollection<GidDisplayItem>();
        private ObservableCollection<string> associationsForCombo = new ObservableCollection<string>();
        private ObservableCollection<GidDisplayItem> relatedByEntityForDisplay = new ObservableCollection<GidDisplayItem>();


        public MainWindow()
        {
            InitializeComponent();

            // Inicijalizacija za prvi tab
            cmbProfile.ItemsSource = Enum.GetValues(typeof(SupportedProfiles));
            cmbProfile.SelectedIndex = 0;

            // Poveži ListView sa ObservableCollection
            lvGids.ItemsSource = loadedGidsForDisplay;

            lvExtentGids.ItemsSource = extentGidsForDisplay;

            cmbEntityType.ItemsSource = Enum.GetValues(typeof(DMSType))
        .Cast<DMSType>()
        .Where(t => t != DMSType.MASK_TYPE)
        .ToList();
            cmbEntityType.SelectedIndex = 0;


            cmbEntityByName.ItemsSource = allEntitiesForCombo;
            cmbAssociationByEntity.ItemsSource = associationsForCombo;
            lvRelatedByEntity.ItemsSource = relatedByEntityForDisplay;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Pozovi metodu za učitavanje GID-ova čim se prozor učita
            LoadInitialGids();
            LoadEntitiesForCombo();
        }


        #region Logika za Prvi Tab (Profile Loader)

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "CIM-XML Files|*.xml;*.txt;*.rdf|All Files|*.*" };
            if (dlg.ShowDialog() == true)
            {
                txtCIMFile.Text = dlg.FileName;
            }
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCIMFile.Text))
            {
                MessageBox.Show("Must enter CIM/XML file.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using (var fs = File.Open(txtCIMFile.Text, FileMode.Open))
                {
                    nmsDelta = adapter.CreateDelta(fs, (SupportedProfiles)cmbProfile.SelectedItem, out string log);
                    txtReport.Text = log;
                }

                if (nmsDelta != null)
                {
                    // Samo omogući dugme i obavesti korisnika
                    btnApplyDelta.IsEnabled = true;
                    MessageBox.Show("Conversion complete. You can now apply the delta.", "Conversion Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during conversion: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnApplyDelta_Click(object sender, RoutedEventArgs e)
        {
            if (nmsDelta == null)
            {
                MessageBox.Show("No delta to apply. Please convert a file first.", "Info");
                return;
            }

            try
            {
                string reportString = adapter.ApplyUpdates(nmsDelta);
                txtReport.AppendText("\n\n" + reportString);

                // Parsiraj report da dobiješ mapu starih i novih GID-ova
                Dictionary<long, long> gidPairs = ParseGidPairsFromReport(reportString);

                if (gidPairs.Count > 0)
                {
                    MessageBox.Show("Delta successfully applied.", "Success");

                    loadedGidsForDisplay.Clear();

                    // Prođi kroz sve parove (stari GID -> novi GID)
                    foreach (var pair in gidPairs)
                    {
                        long oldGid = pair.Key;
                        long newGid = pair.Value;

                        // Pronađi ime za taj objekat iz originalne delte
                        string name = GetNameFromDelta(oldGid);

                        // Kreiraj novi GidDisplayItem i dodaj ga u kolekciju
                        loadedGidsForDisplay.Add(new GidDisplayItem
                        {
                            Gid = newGid,
                            Name = name
                        });
                    }
                }
                else
                {
                    MessageBox.Show("Could not parse server GIDs from the report.", "Parsing Error");
                }

                nmsDelta = null;
                btnApplyDelta.IsEnabled = false;
                LoadEntitiesForCombo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while applying delta:\n\n{ex.Message}", "Error");
            }
        }

        private List<string> ParseServerGidsFromReport(string report)
        {
            var serverGids = new List<string>();
            try
            {
                // Podeli report na linije
                string[] lines = report.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Traži linije koje sadrže "GlobalId pairs"
                bool inPairsSection = false;
                foreach (string line in lines)
                {
                    if (line.Contains("GlobalId pairs:"))
                    {
                        inPairsSection = true;
                        continue;
                    }

                    if (inPairsSection)
                    {
                        // Primer linije: "Client globalId: 0x...     - Server globalId: 0x..."
                        string[] parts = line.Split(new[] { "Server globalId:" }, StringSplitOptions.None);
                        if (parts.Length == 2)
                        {
                            // Uzmi drugi deo i očisti ga
                            string serverGid = parts[1].Trim();
                            serverGids.Add(serverGid);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Loguj grešku ako parsiranje ne uspe
                Console.WriteLine("Error parsing GIDs from report: " + ex.Message);
            }
            return serverGids;
        }

        private void btnGetExtent_Click(object sender, RoutedEventArgs e)
        {
            extentGidsForDisplay.Clear();

            try
            {
                if (cmbEntityType.SelectedItem == null)
                {
                    MessageBox.Show("Izaberi tip entiteta!", "Greška");
                    return;
                }
                DMSType selectedType = (DMSType)cmbEntityType.SelectedItem;
                ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
                ModelCode modelCode = modelResourcesDesc.GetModelCodeFromType(selectedType);

                // Dobavi sve property ID-jevi za taj tip
                List<ModelCode> allProperties = modelResourcesDesc.GetAllPropertyIds(selectedType);

                // Dinamičko pravljenje kolona!
                var gridView = new GridView();
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "GID",
                    DisplayMemberBinding = new Binding("GidHex"),
                    Width = Double.NaN
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Name",
                    DisplayMemberBinding = new Binding("Name"),
                    Width = Double.NaN
                });

                foreach (var prop in allProperties)
                {
                    var propName = prop.ToString();
                    gridView.Columns.Add(new GridViewColumn
                    {
                        Header = propName,
                        DisplayMemberBinding = new Binding($"AttributeValues[{propName}]"),
                        Width = Double.NaN
                    });
                }
                lvExtentGids.View = gridView;

                NetworkModelGDAProxy proxy = null;
                try
                {
                    proxy = new NetworkModelGDAProxy("NetworkModelGDAEndpoint");
                    int iteratorId = proxy.GetExtentValues(modelCode, new List<ModelCode> { ModelCode.IDOBJ_GID, ModelCode.IDOBJ_NAME });

                    int resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = proxy.IteratorNext(100, iteratorId);
                        foreach (var rd in rds)
                        {
                            ResourceDescription fullRd = proxy.GetValues(rd.Id, allProperties);
                            var item = new GidDisplayItem
                            {
                                Gid = rd.Id,
                                Name = rd.GetProperty(ModelCode.IDOBJ_NAME).AsString()
                            };
                            foreach (var prop in allProperties)
                            {
                                var property = fullRd.Properties.FirstOrDefault(p => p.Id == prop);
                                item.AttributeValues[prop.ToString()] = property != null ? property.ToString() : "";
                            }
                            extentGidsForDisplay.Add(item);
                        }
                        resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);
                    }
                    proxy.IteratorClose(iteratorId);
                }
                finally
                {
                    if (proxy != null) { try { proxy.Close(); } catch { proxy.Abort(); } }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška: {ex.Message}", "Greška");
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Logika za Drugi Tab (GDA - Get Values)

        private void lvGids_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lstAttributes.Items.Clear();
            if (lvGids.SelectedItem is GidDisplayItem selectedItem)
            {
                short type = ModelCodeHelper.ExtractTypeFromGlobalId(selectedItem.Gid);
                ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
                List<ModelCode> propertiesToGet = modelResourcesDesc.GetAllPropertyIds((DMSType)type);

                foreach (var prop in propertiesToGet)
                {
                    lstAttributes.Items.Add(new AttributeItem { AttributeName = prop.ToString(), ModelCode = prop });
                }
            }
        }

        private void GetValuesForGid(long gid, List<ModelCode> propertiesToGet)
        {
            txtResult_GetValues.Text = "Fetching selected details...";
            NetworkModelGDAProxy proxy = null;

            try
            {
                proxy = new NetworkModelGDAProxy("NetworkModelGDAEndpoint");
                ResourceDescription rd = proxy.GetValues(gid, propertiesToGet);

                if (rd != null)
                {
                    txtResult_GetValues.Text = FormatResourceDescription(rd);
                }
                else
                {
                    txtResult_GetValues.Text = "Resource not found.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Service Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtResult_GetValues.Text = $"Error: {ex.Message}";
            }
            finally
            {
                if (proxy != null)
                {
                    try { proxy.Close(); } catch { proxy.Abort(); }
                }
            }
        }

        private string FormatResourceDescription(ResourceDescription rd)
        {
            if (rd == null) return "Resource is null.";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Details for GID:{rd.Id}");
            sb.AppendLine("---------------------------------");
            foreach (Property p in rd.Properties)
            {
                switch (p.Type)
                {
                    case PropertyType.Reference:
                        sb.AppendLine($"  - {p.Id}:{p.AsReference()}");
                        break;
                    case PropertyType.ReferenceVector:
                        sb.Append($"  - {p.Id}: [");
                        var refs = p.AsReferences();
                        for (int i = 0; i < refs.Count; i++)
                        {
                            sb.Append($"{refs[i]}");
                            if (i < refs.Count - 1)
                            {
                                sb.Append(", ");
                            }
                        }
                        sb.AppendLine("]");
                        break;
                    default:
                        sb.AppendLine($"  - {p.Id}: {p.ToString()}");
                        break;
                }
            }
            return sb.ToString();
        }

        #endregion

        public class GidDisplayItem
        {
            public long Gid { get; set; }
            public string Name { get; set; }
            public Dictionary<string, object> AttributeValues { get; set; } = new Dictionary<string, object>();
            public string GidHex => $"0x{Gid:X16}";
            public string DisplayText => $"{GidHex} ({Name})";
        }


        private void LoadInitialGids()
        {
            loadedGidsForDisplay.Clear();
            txtResult_GetValues.Text = "Loading initial GIDs from server...";

            try
            {
                // Prođi kroz sve tipove definisane u DMSType enumu
                foreach (DMSType dmsType in Enum.GetValues(typeof(DMSType)))
                {
                    // Preskoči MASK_TYPE ako postoji
                    if (dmsType == DMSType.MASK_TYPE) continue;

                    NetworkModelGDAProxy proxy = null;
                    try
                    {
                        proxy = new NetworkModelGDAProxy("NetworkModelGDAEndpoint");

                        ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
                        ModelCode modelCode = modelResourcesDesc.GetModelCodeFromType(dmsType);
                        List<ModelCode> properties = new List<ModelCode>() { ModelCode.IDOBJ_GID, ModelCode.IDOBJ_NAME };

                        int iteratorId = proxy.GetExtentValues(modelCode, properties);
                        int resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);

                        while (resourcesLeft > 0)
                        {
                            List<ResourceDescription> rds = proxy.IteratorNext(100, iteratorId);
                            foreach (var rd in rds)
                            {
                                // KREIRAJ GidDisplayItem
                                loadedGidsForDisplay.Add(new GidDisplayItem
                                {
                                    Gid = rd.Id,
                                    Name = rd.GetProperty(ModelCode.IDOBJ_NAME).AsString()
                                });
                            }
                            resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);
                        }
                    }
                    finally
                    {
                        if (proxy != null)
                        {
                            try { proxy.Close(); } catch { proxy.Abort(); }
                        }
                    }
                }
                txtResult_GetValues.Text = $"{loadedGidsForDisplay.Count} GIDs loaded. Select one to see details.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load initial GIDs: {ex.Message}", "Error");
                txtResult_GetValues.Text = $"Error loading GIDs: {ex.Message}";
            }
        }

        private Dictionary<long, long> ParseGidPairsFromReport(string report)
        {
            var gidPairs = new Dictionary<long, long>();
            try
            {
                string[] lines = report.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                bool inPairsSection = false;

                foreach (string line in lines)
                {
                    if (line.Contains("GlobalId pairs:"))
                    {
                        inPairsSection = true;
                        continue;
                    }

                    if (inPairsSection)
                    {
                        string[] parts = line.Split(new[] { "Client globalId:", "- Server globalId:" }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            long clientGid = Convert.ToInt64(parts[0].Trim().Substring(2), 16);
                            long serverGid = Convert.ToInt64(parts[1].Trim().Substring(2), 16);
                            gidPairs[clientGid] = serverGid;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing GID pairs from report: " + ex.Message);
            }
            return gidPairs;
        }

        private string GetNameFromDelta(long oldGid)
        {
            var rd = nmsDelta?.InsertOperations.FirstOrDefault(op => op.Id == oldGid);

            if (rd != null && rd.ContainsProperty(ModelCode.IDOBJ_NAME))
            {
                return rd.GetProperty(ModelCode.IDOBJ_NAME).AsString();
            }
            return "N/A";
        }


        private void LoadEntitiesForCombo()
        {
            allEntitiesForCombo.Clear();

            foreach (DMSType dmsType in Enum.GetValues(typeof(DMSType)))
            {
                if (dmsType == DMSType.MASK_TYPE) continue;

                NetworkModelGDAProxy proxy = null;
                try
                {
                    proxy = new NetworkModelGDAProxy("NetworkModelGDAEndpoint");
                    ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
                    ModelCode modelCode = modelResourcesDesc.GetModelCodeFromType(dmsType);
                    List<ModelCode> properties = new List<ModelCode>() { ModelCode.IDOBJ_GID, ModelCode.IDOBJ_NAME };

                    int iteratorId = proxy.GetExtentValues(modelCode, properties);
                    int resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = proxy.IteratorNext(100, iteratorId);
                        foreach (var rd in rds)
                        {
                            allEntitiesForCombo.Add(new GidDisplayItem
                            {
                                Gid = rd.Id,
                                Name = rd.GetProperty(ModelCode.IDOBJ_NAME).AsString()
                            });
                        }
                        resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);
                    }
                    proxy.IteratorClose(iteratorId);
                }
                finally
                {
                    if (proxy != null) { try { proxy.Close(); } catch { proxy.Abort(); } }
                }
            }
        }


        private void cmbEntityByName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            associationsForCombo.Clear();

            if (cmbEntityByName.SelectedItem is GidDisplayItem selectedEntity)
            {
                short type = ModelCodeHelper.ExtractTypeFromGlobalId(selectedEntity.Gid);

                switch ((DMSType)type)
                {
                    case DMSType.TERMINAL:
                        associationsForCombo.Add("Terminal → RegulatingControl");
                        break;
                    case DMSType.REGULATINGCONTROL:
                        associationsForCombo.Add("RegulatingControl → Terminal");
                        associationsForCombo.Add("RegulatingControl → RegulationSchedule");
                        associationsForCombo.Add("RegulatingControl → RegulatingCondEq");
                        break;
                    case DMSType.REGULATIONSCHEDULE:
                        associationsForCombo.Add("RegulationSchedule → RegulatingControl");
                        associationsForCombo.Add("RegulationSchedule → DayType");
                        break;
                    case DMSType.STATICVARCOMPENSATOR:
                        associationsForCombo.Add("StaticVarCompensator → RegulatingControl");
                        break;
                    case DMSType.SHUNTCOMPENSATOR:
                        associationsForCombo.Add("ShuntCompensator → RegulatingControl");
                        break;
                    case DMSType.DAYTYPE:
                        associationsForCombo.Add("DayType → RegulationSchedule");
                        break;

                }
                if (associationsForCombo.Count > 0)
                    cmbAssociationByEntity.SelectedIndex = 0;
            }
        }



        private void btnGetRelatedByEntity_Click(object sender, RoutedEventArgs e)
        {
            relatedByEntityForDisplay.Clear();

            if (cmbEntityByName.SelectedItem is GidDisplayItem selectedEntity && cmbAssociationByEntity.SelectedItem is string selectedAssoc)
            {
                Association association = GetAssociationFromSelection(selectedAssoc);
                if (association == null)
                {
                    MessageBox.Show("Selected association is not implemented yet.", "Info");
                    return;
                }

                ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
                NetworkModelGDAProxy proxy = null;
                try
                {
                    proxy = new NetworkModelGDAProxy("NetworkModelGDAEndpoint");

                    // Specijalni slučaj za RegulatingControl → RegulatingCondEq
                    if (selectedAssoc == "RegulatingControl → RegulatingCondEq")
                    {
                        long regControlGid = selectedEntity.Gid;
                        List<DMSType> concreteTypes = new List<DMSType>
                {
                    DMSType.SHUNTCOMPENSATOR,
                    DMSType.STATICVARCOMPENSATOR
                    // Dodaj još podtipova ako ih imaš!
                };

                        GridView gridView = new GridView();
                        gridView.Columns.Add(new GridViewColumn
                        {
                            Header = "GID",
                            DisplayMemberBinding = new Binding("GidHex"),
                            Width = Double.NaN
                        });
                        gridView.Columns.Add(new GridViewColumn
                        {
                            Header = "Name",
                            DisplayMemberBinding = new Binding("Name"),
                            Width = Double.NaN
                        });

                        bool columnsSet = false;

                        foreach (var dmsType in concreteTypes)
                        {
                            ModelCode modelCode = modelResourcesDesc.GetModelCodeFromType(dmsType);
                            List<ModelCode> allProperties = modelResourcesDesc.GetAllPropertyIds(dmsType);

                            int iteratorId = proxy.GetExtentValues(modelCode, new List<ModelCode> { ModelCode.IDOBJ_GID, ModelCode.IDOBJ_NAME, ModelCode.REG_CONEQ_RG_CNTRL });
                            int resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);

                            while (resourcesLeft > 0)
                            {
                                List<ResourceDescription> rds = proxy.IteratorNext(100, iteratorId);
                                foreach (var rd in rds)
                                {
                                    var refProp = rd.GetProperty(ModelCode.REG_CONEQ_RG_CNTRL);
                                    if (refProp != null && refProp.AsReference() == regControlGid)
                                    {
                                        if (!columnsSet)
                                        {
                                            foreach (var prop in allProperties)
                                            {
                                                var propName = prop.ToString();
                                                gridView.Columns.Add(new GridViewColumn
                                                {
                                                    Header = propName,
                                                    DisplayMemberBinding = new Binding($"AttributeValues[{propName}]"),
                                                    Width = Double.NaN
                                                });
                                            }
                                            lvRelatedByEntity.View = gridView;
                                            columnsSet = true;
                                        }

                                        ResourceDescription fullRd = proxy.GetValues(rd.Id, allProperties);
                                        var item = new GidDisplayItem
                                        {
                                            Gid = rd.Id,
                                            Name = rd.GetProperty(ModelCode.IDOBJ_NAME).AsString()
                                        };
                                        foreach (var prop in allProperties)
                                        {
                                            var property = fullRd.Properties.FirstOrDefault(p => p.Id == prop);
                                            item.AttributeValues[prop.ToString()] = property != null ? property.ToString() : "";
                                        }
                                        relatedByEntityForDisplay.Add(item);
                                    }
                                }
                                resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);
                            }
                            proxy.IteratorClose(iteratorId);
                        }
                    }
                    // Specijalni slučaj za DayType → RegulationSchedule
                    else if (selectedAssoc == "DayType → RegulationSchedule")
                    {
                        long dayTypeGid = selectedEntity.Gid;

                        ModelCode scheduleModelCode = modelResourcesDesc.GetModelCodeFromType(DMSType.REGULATIONSCHEDULE);
                        List<ModelCode> allProperties = modelResourcesDesc.GetAllPropertyIds(DMSType.REGULATIONSCHEDULE);

                        // Property koji RegulationSchedule koristi za vezu ka DayType
                        ModelCode referenceProp = ModelCode.SEASON_DAYTYPE_DAYTYPE; // Zameni ako koristiš drugi property!

                        int iteratorId = proxy.GetExtentValues(scheduleModelCode, new List<ModelCode> { ModelCode.IDOBJ_GID, ModelCode.IDOBJ_NAME, referenceProp });
                        int resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);

                        GridView gridView = new GridView();
                        gridView.Columns.Add(new GridViewColumn
                        {
                            Header = "GID",
                            DisplayMemberBinding = new Binding("GidHex"),
                            Width = Double.NaN
                        });
                        gridView.Columns.Add(new GridViewColumn
                        {
                            Header = "Name",
                            DisplayMemberBinding = new Binding("Name"),
                            Width = Double.NaN
                        });

                        bool columnsSet = false;

                        while (resourcesLeft > 0)
                        {
                            List<ResourceDescription> rds = proxy.IteratorNext(100, iteratorId);
                            foreach (var rd in rds)
                            {
                                var refProp = rd.GetProperty(referenceProp);
                                if (refProp != null && refProp.AsReference() == dayTypeGid)
                                {
                                    if (!columnsSet)
                                    {
                                        foreach (var prop in allProperties)
                                        {
                                            var propName = prop.ToString();
                                            gridView.Columns.Add(new GridViewColumn
                                            {
                                                Header = propName,
                                                DisplayMemberBinding = new Binding($"AttributeValues[{propName}]"),
                                                Width = Double.NaN
                                            });
                                        }
                                        lvRelatedByEntity.View = gridView;
                                        columnsSet = true;
                                    }

                                    ResourceDescription fullRd = proxy.GetValues(rd.Id, allProperties);
                                    var item = new GidDisplayItem
                                    {
                                        Gid = rd.Id,
                                        Name = rd.GetProperty(ModelCode.IDOBJ_NAME).AsString()
                                    };
                                    foreach (var prop in allProperties)
                                    {
                                        var property = fullRd.Properties.FirstOrDefault(p => p.Id == prop);
                                        item.AttributeValues[prop.ToString()] = property != null ? property.ToString() : "";
                                    }
                                    relatedByEntityForDisplay.Add(item);
                                }
                            }
                            resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);
                        }
                        proxy.IteratorClose(iteratorId);
                    }
                    // Standardni slučaj za ostale asocijacije
                    else
                    {
                        List<ModelCode> basicProperties = new List<ModelCode> { ModelCode.IDOBJ_GID, ModelCode.IDOBJ_NAME };
                        int iteratorId = proxy.GetRelatedValues(selectedEntity.Gid, basicProperties, association);
                        int resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);

                        GridView gridView = new GridView();
                        gridView.Columns.Add(new GridViewColumn
                        {
                            Header = "GID",
                            DisplayMemberBinding = new Binding("GidHex"),
                            Width = Double.NaN
                        });
                        gridView.Columns.Add(new GridViewColumn
                        {
                            Header = "Name",
                            DisplayMemberBinding = new Binding("Name"),
                            Width = Double.NaN
                        });

                        List<ModelCode> allProperties = null;
                        bool columnsSet = false;

                        while (resourcesLeft > 0)
                        {
                            List<ResourceDescription> rds = proxy.IteratorNext(100, iteratorId);
                            foreach (var rd in rds)
                            {
                                short type = ModelCodeHelper.ExtractTypeFromGlobalId(rd.Id);
                                allProperties = modelResourcesDesc.GetAllPropertyIds((DMSType)type);

                                if (!columnsSet)
                                {
                                    foreach (var prop in allProperties)
                                    {
                                        var propName = prop.ToString();
                                        gridView.Columns.Add(new GridViewColumn
                                        {
                                            Header = propName,
                                            DisplayMemberBinding = new Binding($"AttributeValues[{propName}]"),
                                            Width = Double.NaN
                                        });
                                    }
                                    lvRelatedByEntity.View = gridView;
                                    columnsSet = true;
                                }

                                ResourceDescription fullRd = proxy.GetValues(rd.Id, allProperties);
                                var item = new GidDisplayItem
                                {
                                    Gid = rd.Id,
                                    Name = rd.GetProperty(ModelCode.IDOBJ_NAME).AsString()
                                };
                                foreach (var prop in allProperties)
                                {
                                    var property = fullRd.Properties.FirstOrDefault(p => p.Id == prop);
                                    item.AttributeValues[prop.ToString()] = property != null ? property.ToString() : "";
                                }
                                relatedByEntityForDisplay.Add(item);
                            }
                            resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);
                        }
                        proxy.IteratorClose(iteratorId);
                    }
                }
                finally
                {
                    if (proxy != null) { try { proxy.Close(); } catch { proxy.Abort(); } }
                }
            }
            else
            {
                MessageBox.Show("Select entity and association first.", "Info");
            }
        }


        private Association GetAssociationFromSelection(string selectedAssoc)
        {
            switch (selectedAssoc)
            {
                case "Terminal → RegulatingControl":
                    return new Association(ModelCode.TRM_REG_CNTRL, ModelCode.RG_CNTRL);
                case "RegulatingControl → Terminal":
                    return new Association(ModelCode.RG_CNTRL_TRM, ModelCode.TRM);
                case "RegulatingControl → RegulationSchedule":
                    return new Association(ModelCode.RG_CNTRL_REG_SCHEDULE, ModelCode.REG_SCHEDULE);
                case "RegulatingControl → RegulatingCondEq":
                    return new Association(ModelCode.RG_CNTRL_REG_CONEQ, ModelCode.REG_CONEQ);
                case "RegulationSchedule → RegulatingControl":
                    return new Association(ModelCode.REG_SCHEDULE_RG_CNTRL, ModelCode.RG_CNTRL);
                case "StaticVarCompensator → RegulatingControl":
                    return new Association(ModelCode.REG_CONEQ_RG_CNTRL, ModelCode.RG_CNTRL);
                case "ShuntCompensator → RegulatingControl":
                    return new Association(ModelCode.REG_CONEQ_RG_CNTRL, ModelCode.RG_CNTRL);
                case "DayType → RegulationSchedule":
                    return new Association(ModelCode.DAYTYPE_SEASON_DAYTYPE, ModelCode.SEASON_DAYTYPE);
                case "RegulationSchedule → DayType":
                    return new Association(ModelCode.SEASON_DAYTYPE_DAYTYPE, ModelCode.DAYTYPE);
                default:
                    return null;
            }
        }


        private void btnGetSelectedValues_Click(object sender, RoutedEventArgs e)
        {
            if (lvGids.SelectedItem is GidDisplayItem selectedItem)
            {
                var selectedAttributes = lstAttributes.SelectedItems.Cast<AttributeItem>().Select(a => a.ModelCode).ToList();
                if (selectedAttributes.Count == 0)
                {
                    MessageBox.Show("Select attributes to fetch!", "Info");
                    return;
                }
                GetValuesForGid(selectedItem.Gid, selectedAttributes);
            }
        }



        public class AttributeItem
        {
            public string AttributeName { get; set; }
            public ModelCode ModelCode { get; set; }
        }


        private List<DMSType> GetConcreteRegulatingCondEqTypes()
        {
            return new List<DMSType> {
                DMSType.SHUNTCOMPENSATOR,
                DMSType.STATICVARCOMPENSATOR
                // Dodaj još konkretnih naslednika ako ih imaš!
            };
        }

        private List<GidDisplayItem> FetchCondEqForRegulatingControl(long regControlGid)
        {
            var result = new List<GidDisplayItem>();
            ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
            NetworkModelGDAProxy proxy = new NetworkModelGDAProxy("NetworkModelGDAEndpoint");

            foreach (var dmsType in GetConcreteRegulatingCondEqTypes())
            {
                ModelCode modelCode = modelResourcesDesc.GetModelCodeFromType(dmsType);
                List<ModelCode> properties = new List<ModelCode> { ModelCode.IDOBJ_GID, ModelCode.IDOBJ_NAME, ModelCode.REG_CONEQ_RG_CNTRL };
                int iteratorId = proxy.GetExtentValues(modelCode, properties);
                int resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = proxy.IteratorNext(100, iteratorId);
                    foreach (var rd in rds)
                    {
                        if (rd.GetProperty(ModelCode.REG_CONEQ_RG_CNTRL).AsReference() == regControlGid)
                        {
                            result.Add(new GidDisplayItem
                            {
                                Gid = rd.Id,
                                Name = rd.GetProperty(ModelCode.IDOBJ_NAME).AsString()
                            });
                        }
                    }
                    resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);
                }
                proxy.IteratorClose(iteratorId);
            }
            proxy.Close();
            return result;
        }


    }
}