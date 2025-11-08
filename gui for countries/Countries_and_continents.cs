using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ContinentsCountriesApp
{
    public class Country
    {
        public string Name { get; set; }
        public string Capital { get; set; }
        public string Population { get; set; }
        public string Area { get; set; }
        public string Currency { get; set; }
        public string FlagUrl { get; set; }
    }

    // Custom styled panel with gradient background
    public class GradientPanel : Panel
    {
        private Color startColor = Color.FromArgb(240, 248, 255);
        private Color endColor = Color.FromArgb(173, 216, 230);

        [System.ComponentModel.Browsable(true)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color StartColor 
        { 
            get { return startColor; }
            set 
            { 
                startColor = value;
                this.Invalidate();
            }
        }

        [System.ComponentModel.Browsable(true)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color EndColor 
        { 
            get { return endColor; }
            set 
            { 
                endColor = value;
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                StartColor,
                EndColor,
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
            base.OnPaint(e);
        }
    }

    // Custom styled ListBox
    public class StyledListBox : ListBox
    {
        public StyledListBox()
        {
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.ItemHeight = 30;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            // Highlight selected item with custom color
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(70, 130, 180)))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }

            // Draw text
            using (SolidBrush textBrush = new SolidBrush(
                (e.State & DrawItemState.Selected) == DrawItemState.Selected ? Color.White : Color.Black))
            {
                e.Graphics.DrawString(
                    this.Items[e.Index].ToString(),
                    e.Font,
                    textBrush,
                    e.Bounds.Left + 10,
                    e.Bounds.Top + 7);
            }

            e.DrawFocusRectangle();
        }
    }

    public partial class MainForm : Form
    {
        private ComboBox cmbContinents;
        private StyledListBox lstCountries;
        private Label lblName, lblCapital, lblPopulation, lblArea, lblCurrency;
        private PictureBox picFlag;
        private Dictionary<string, List<Country>> continentData;

        public MainForm()
        {
            InitializeComponent();
            LoadData();
            LoadContinents();
        }

        private void InitializeComponent()
        {
            this.Text = "Continents and Countries Information System";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Gradient background for main form
            this.BackColor = Color.FromArgb(245, 245, 250);

            // Create main gradient panel
            GradientPanel mainPanel = new GradientPanel
            {
                Dock = DockStyle.Fill,
                StartColor = Color.FromArgb(230, 240, 255),
                EndColor = Color.FromArgb(255, 250, 240)
            };
            this.Controls.Add(mainPanel);

            // Title Label centered across the top
            Label titleLabel = new Label
            {
                Text = "🌍 WORLD CONTINENTS & COUNTRIES",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = false,
                ForeColor = Color.FromArgb(25, 25, 112),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            // Stretch label to full width and keep it centered on resize
            titleLabel.Location = new Point(0, 25);
            titleLabel.Size = new Size(mainPanel.ClientSize.Width, 40);
            titleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            mainPanel.Controls.Add(titleLabel);

            // Decorative line under title
            Panel titleUnderline = new Panel
            {
                Location = new Point(220, 68),
                Size = new Size(500, 3),
                BackColor = Color.FromArgb(70, 130, 180)
            };
            mainPanel.Controls.Add(titleUnderline);

            // Left panel container with rounded effect
            Panel leftPanel = new Panel
            {
                Location = new Point(25, 90),
                Size = new Size(280, 490),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            AddPanelShadow(leftPanel);
            mainPanel.Controls.Add(leftPanel);

            // Continent Selection
            Label lblContinentSelect = new Label
            {
                Text = "🗺️ Select Continent:",
                Location = new Point(15, 15),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(47, 79, 79),
                BackColor = Color.Transparent
            };
            leftPanel.Controls.Add(lblContinentSelect);

            cmbContinents = new ComboBox
            {
                Location = new Point(15, 45),
                Size = new Size(250, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 248, 255)
            };
            cmbContinents.SelectedIndexChanged += CmbContinents_SelectedIndexChanged;
            leftPanel.Controls.Add(cmbContinents);

            // Countries List
            Label lblCountriesList = new Label
            {
                Text = "📍 Countries:",
                Location = new Point(15, 90),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(47, 79, 79),
                BackColor = Color.Transparent
            };
            leftPanel.Controls.Add(lblCountriesList);

            lstCountries = new StyledListBox
            {
                Location = new Point(15, 120),
                Size = new Size(250, 355),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 248, 255)
            };
            lstCountries.SelectedIndexChanged += LstCountries_SelectedIndexChanged;
            leftPanel.Controls.Add(lstCountries);

            // Country Information Panel with enhanced styling
            Panel infoPanel = new Panel
            {
                Location = new Point(325, 90),
                Size = new Size(590, 490),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            AddPanelShadow(infoPanel);
            mainPanel.Controls.Add(infoPanel);

            Label lblInfoTitle = new Label
            {
                Text = "COUNTRY INFORMATION",
                Location = new Point(160, 15),
                Size = new Size(270, 30),
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 25, 112),
                BackColor = Color.Transparent
            };
            infoPanel.Controls.Add(lblInfoTitle);

            // Decorative line
            Panel infoUnderline = new Panel
            {
                Location = new Point(160, 48),
                Size = new Size(270, 2),
                BackColor = Color.FromArgb(70, 130, 180)
            };
            infoPanel.Controls.Add(infoUnderline);

            // Flag with styled border
            Panel flagContainer = new Panel
            {
                Location = new Point(165, 70),
                Size = new Size(260, 150),
                BackColor = Color.FromArgb(240, 248, 255),
                BorderStyle = BorderStyle.None
            };
            infoPanel.Controls.Add(flagContainer);

            picFlag = new PictureBox
            {
                Location = new Point(5, 5),
                Size = new Size(250, 140),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };
            flagContainer.Controls.Add(picFlag);

            // Information fields with icons and styled layout
            int yPos = 240;
            int spacing = 45;

            // Country Name
            CreateInfoField(infoPanel, "🏳️ Country:", ref lblName, yPos);
            yPos += spacing;

            // Capital
            CreateInfoField(infoPanel, "🏛️ Capital:", ref lblCapital, yPos);
            yPos += spacing;

            // Population
            CreateInfoField(infoPanel, "👥 Population:", ref lblPopulation, yPos);
            yPos += spacing;

            // Area
            CreateInfoField(infoPanel, "📏 Area:", ref lblArea, yPos);
            yPos += spacing;

            // Currency
            CreateInfoField(infoPanel, "💰 Currency:", ref lblCurrency, yPos);
        }

        private void CreateInfoField(Panel parent, string labelText, ref Label valueLabel, int yPosition)
        {
            Label titleLabel = new Label
            {
                Text = labelText,
                Location = new Point(40, yPosition),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(47, 79, 79),
                BackColor = Color.Transparent
            };
            parent.Controls.Add(titleLabel);

            valueLabel = new Label
            {
                Location = new Point(220, yPosition),
                Size = new Size(350, 25),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(0, 0, 0),
                BackColor = Color.Transparent
            };
            parent.Controls.Add(valueLabel);
        }

        private void AddPanelShadow(Panel panel)
        {
            // Simulate shadow with a border
            panel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
                }
            };
        }

        private void LoadData()
        {
            continentData = new Dictionary<string, List<Country>>();

            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string jsonPath = Path.Combine(exeDir, "countries_by_continent.json");

                if (!File.Exists(jsonPath))
                {
                    string alt = Path.GetFullPath(Path.Combine(exeDir, "..", "..", "..", "..", "countries_by_continent.json"));
                    if (File.Exists(alt)) jsonPath = alt;
                }

                if (File.Exists(jsonPath))
                {
                    string json = File.ReadAllText(jsonPath);

                    var root = JsonNode.Parse(json) as JsonObject;
                    if (root != null)
                    {
                        foreach (var kvp in root)
                        {
                            var continentName = kvp.Key;
                            var value = kvp.Value as JsonArray;
                            if (value == null) continue;

                            var list = new List<Country>();
                            foreach (var item in value)
                            {
                                if (item is JsonValue)
                                {
                                    var nameStr = item?.GetValue<string>();
                                    if (!string.IsNullOrWhiteSpace(nameStr))
                                    {
                                        list.Add(new Country { Name = nameStr });
                                    }
                                }
                                else if (item is JsonObject obj)
                                {
                                    string name = obj["name"]?.GetValue<string>() ?? string.Empty;
                                    string capital = obj["capital"]?.GetValue<string>() ?? string.Empty;
                                    string currency = obj["currency"]?.GetValue<string>() ?? string.Empty;
                                    string flagUrlVal = obj["flagUrl"]?.GetValue<string>()
                                                       ?? obj["FlagUrl"]?.GetValue<string>()
                                                       ?? string.Empty;

                                    string populationStr = string.Empty;
                                    if (obj.TryGetPropertyValue("population", out var popNode))
                                    {
                                        if (popNode is JsonValue popVal)
                                        {
                                            if (popVal.TryGetValue<long>(out var popLong))
                                                populationStr = popLong.ToString("N0");
                                            else if (popVal.TryGetValue<double>(out var popDbl))
                                                populationStr = ((long)popDbl).ToString("N0");
                                            else if (popVal.TryGetValue<string>(out var popStr))
                                                populationStr = popStr;
                                            else
                                                populationStr = popVal.ToJsonString();
                                        }
                                    }

                                    string areaStr = string.Empty;
                                    if (obj.TryGetPropertyValue("area_km2", out var areaNode))
                                    {
                                        if (areaNode is JsonValue areaVal)
                                        {
                                            if (areaVal.TryGetValue<double>(out var areaDbl))
                                                areaStr = string.Format("{0:N0} km²", areaDbl);
                                            else if (areaVal.TryGetValue<long>(out var areaLong))
                                                areaStr = string.Format("{0:N0} km²", areaLong);
                                            else if (areaVal.TryGetValue<string>(out var areaStrRaw))
                                                areaStr = areaStrRaw;
                                            else
                                                areaStr = areaVal.ToJsonString();
                                        }
                                    }

                                    list.Add(new Country
                                    {
                                        Name = name,
                                        Capital = capital,
                                        Currency = currency,
                                        Population = populationStr,
                                        Area = areaStr,
                                        FlagUrl = flagUrlVal
                                    });
                                }
                            }

                            continentData[continentName] = list;
                        }
                    }
                }
            }
            catch
            {
                continentData = new Dictionary<string, List<Country>>();
            }
        }

        private void LoadContinents()
        {
            cmbContinents.Items.Clear();
            foreach (var continent in continentData.Keys)
            {
                cmbContinents.Items.Add(continent);
            }

            if (cmbContinents.Items.Count > 0)
                cmbContinents.SelectedIndex = 0;
        }

        private void CmbContinents_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstCountries.Items.Clear();
            string selectedContinent = cmbContinents.SelectedItem.ToString();
            
            if (continentData.ContainsKey(selectedContinent))
            {
                foreach (var country in continentData[selectedContinent])
                {
                    lstCountries.Items.Add(country.Name);
                }
            }

            ClearCountryDetails();
        }

        private void LstCountries_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstCountries.SelectedItem != null && cmbContinents.SelectedItem != null)
            {
                string selectedCountry = lstCountries.SelectedItem.ToString();
                string selectedContinent = cmbContinents.SelectedItem.ToString();
                
                Country country = continentData[selectedContinent].Find(c => c.Name == selectedCountry);
                
                if (country != null)
                {
                    DisplayCountryDetails(country);
                }
            }
        }

        private void DisplayCountryDetails(Country country)
        {
            lblName.Text = country.Name;
            lblCapital.Text = country.Capital;
            lblPopulation.Text = country.Population;
            lblArea.Text = country.Area;
            lblCurrency.Text = country.Currency;

            try
            {
                using (WebClient client = new WebClient())
                {
                    if (!string.IsNullOrWhiteSpace(country.FlagUrl))
                    {
                        byte[] imageData = client.DownloadData(country.FlagUrl);
                        using (var ms = new System.IO.MemoryStream(imageData))
                        {
                            picFlag.Image = Image.FromStream(ms);
                        }
                    }
                }
            }
            catch
            {
                picFlag.Image = null;
            }
        }

        private void ClearCountryDetails()
        {
            lblName.Text = "";
            lblCapital.Text = "";
            lblPopulation.Text = "";
            lblArea.Text = "";
            lblCurrency.Text = "";
            picFlag.Image = null;
        }
    }

    public class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}