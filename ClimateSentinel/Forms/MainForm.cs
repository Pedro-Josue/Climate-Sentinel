using ClimateSentinel.Exceptions;
using ClimateSentinel.Interfaces;
using ClimateSentinel.Models;
using ClimateSentinel.Providers;
using ClimateSentinel.Services;
using ClimateSentinel.Utils;

namespace ClimateSentinel.Forms;

public sealed class MainForm : Form
{
    private const int SidebarWidth = 460;

    private readonly IClimateProvider _climateProvider;
    private readonly ClimateAnalysisService _analysisService;
    private readonly List<CidadeMonitorada> _cities;
    private readonly List<MonitoramentoRegistro> _history = new();
    private readonly List<AlertaClimatico> _alerts = new();
    private readonly List<Button> _navButtons = new();

    private ComboBox _cityComboBox = null!;
    private Button _updateButton = null!;
    private Label _selectedCityLabel = null!;
    private Label _temperatureValue = null!;
    private Label _humidityValue = null!;
    private Label _rainValue = null!;
    private Label _windValue = null!;
    private Label _riskValue = null!;
    private Label _lastUpdateValue = null!;
    private Label _reportSummaryLabel = null!;
    private TextBox _reportTextBox = null!;
    private DataGridView _historyGrid = null!;
    private DataGridView _alertsGrid = null!;
    private TabControl _tabs = null!;

    private bool _initialLoadAttempted;

    public MainForm()
    {
        _climateProvider = new OpenMeteoProvider();
        _analysisService = new ClimateAnalysisService();
        _cities = CityCatalog.ObterCidades().ToList();

        Text = "Climate Sentinel";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1280, 820);
        BackColor = UiPalette.Background;
        ForeColor = UiPalette.TextPrimary;
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildUi();
        BindEvents();
        RefreshUi();

        Shown += async (_, _) => await EnsureInitialDataAsync();
        FormClosed += (_, _) =>
        {
            if (_climateProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        };
    }

    private void BuildUi()
    {
        var root = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = SidebarWidth,
            BackColor = UiPalette.Background,
            FixedPanel = FixedPanel.Panel1,
            IsSplitterFixed = true,
            BorderStyle = BorderStyle.None,
            Panel1MinSize = SidebarWidth,
            SplitterWidth = 1
        };
        Controls.Add(root);
        root.SplitterDistance = SidebarWidth;
        root.SizeChanged += (_, _) => root.SplitterDistance = SidebarWidth;

        BuildSidebar(root.Panel1);
        BuildMainContent(root.Panel2);
    }

    private void BuildSidebar(Control host)
    {
        host.BackColor = UiPalette.Sidebar;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(16, 18, 16, 16),
            BackColor = UiPalette.Sidebar
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        host.Controls.Add(layout);

        var title = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Climate Sentinel",
            ForeColor = UiPalette.TextPrimary,
            Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.Controls.Add(title, 0, 0);

        var subtitle = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Monitoramento climático",
            ForeColor = UiPalette.TextSecondary,
            Font = new Font("Segoe UI", 9.25F),
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.Controls.Add(subtitle, 0, 1);

        var nav = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 18, 0, 0),
            AutoScroll = false,
            BackColor = UiPalette.Sidebar
        };
        layout.Controls.Add(nav, 0, 2);

        nav.Controls.Add(CreateNavButton("Home", 0));
        nav.Controls.Add(CreateNavButton("Histórico", 1));
        nav.Controls.Add(CreateNavButton("Alertas", 2));
        nav.Controls.Add(CreateNavButton("Relatórios", 3));
    }

    private Button CreateNavButton(string text, int tabIndex)
    {
        var button = new Button
        {
            AutoSize = false,
            Width = SidebarWidth - 48,
            Height = 52,
            Text = text,
            ForeColor = UiPalette.TextSecondary,
            BackColor = UiPalette.SurfaceLight,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(14, 0, 14, 0),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Tag = tabIndex
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = UiPalette.Surface;
        button.FlatAppearance.MouseDownBackColor = UiPalette.Accent;
        button.Click += (_, _) =>
        {
            _tabs.SelectedIndex = tabIndex;
            UpdateNavigationState(tabIndex);
        };
        _navButtons.Add(button);
        return button;
    }

    private void UpdateNavigationState(int selectedIndex)
    {
        foreach (var button in _navButtons)
        {
            var isSelected = button.Tag is int tabIndex && tabIndex == selectedIndex;
            button.BackColor = isSelected ? UiPalette.Accent : UiPalette.SurfaceLight;
        }
    }

    private void BuildMainContent(Control host)
    {
        host.BackColor = UiPalette.Background;
        host.Padding = new Padding(16, 14, 16, 14);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiPalette.Background
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        host.Controls.Add(layout);

        layout.Controls.Add(BuildHeaderCard(), 0, 0);
        layout.Controls.Add(BuildContentTabs(), 0, 1);
    }

    private Control BuildHeaderCard()
    {
        var card = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Dashboard principal",
            ForeColor = UiPalette.TextSecondary,
            BackColor = UiPalette.Surface,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
        };

        var headerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(12)
        };
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        card.Controls.Add(headerLayout);

        var cityLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Cidade selecionada",
            ForeColor = UiPalette.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft
        };
        headerLayout.Controls.Add(cityLabel, 0, 0);

        var updateLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Última atualização",
            ForeColor = UiPalette.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft
        };
        headerLayout.Controls.Add(updateLabel, 1, 0);

        _selectedCityLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "-",
            ForeColor = UiPalette.TextPrimary,
            Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        headerLayout.Controls.Add(_selectedCityLabel, 0, 1);

        _lastUpdateValue = new Label
        {
            Dock = DockStyle.Fill,
            Text = "-",
            ForeColor = UiPalette.TextPrimary,
            Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        headerLayout.Controls.Add(_lastUpdateValue, 1, 1);

        return card;
    }

    private Control BuildContentTabs()
    {
        _tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Appearance = TabAppearance.FlatButtons,
            BackColor = UiPalette.Background,
            ItemSize = new Size(0, 1),
            SizeMode = TabSizeMode.Fixed
        };

        var monitoringTab = new TabPage("Monitoramento")
        {
            BackColor = UiPalette.Background
        };
        monitoringTab.Controls.Add(BuildMonitoringSection());

        var historyTab = new TabPage("Histórico")
        {
            BackColor = UiPalette.Background
        };
        historyTab.Controls.Add(BuildHistorySection());

        var alertsTab = new TabPage("Alertas")
        {
            BackColor = UiPalette.Background
        };
        alertsTab.Controls.Add(BuildAlertsSection());

        var reportsTab = new TabPage("Relatórios")
        {
            BackColor = UiPalette.Background
        };
        reportsTab.Controls.Add(BuildReportsSection());

        _tabs.TabPages.Add(monitoringTab);
        _tabs.TabPages.Add(historyTab);
        _tabs.TabPages.Add(alertsTab);
        _tabs.TabPages.Add(reportsTab);
        _tabs.SelectedIndexChanged += (_, _) => UpdateNavigationState(_tabs.SelectedIndex);
        UpdateNavigationState(0);
        return _tabs;
    }

    private Control BuildMonitoringSection()
    {
        var panel = BuildSectionPanel();

        var top = new GroupBox
        {
            Dock = DockStyle.Top,
            Height = 115,
            Text = "Seleção de cidade",
            ForeColor = UiPalette.TextSecondary,
            BackColor = UiPalette.Surface,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
        };
        panel.Controls.Add(top);

        var topLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            Padding = new Padding(12)
        };
        topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260F));
        topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
        topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        top.Controls.Add(topLayout);

        _cityComboBox = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = UiPalette.Sidebar,
            ForeColor = UiPalette.TextPrimary,
            FlatStyle = FlatStyle.Flat
        };
        _cityComboBox.Items.AddRange(_cities.Cast<object>().ToArray());
        topLayout.Controls.Add(_cityComboBox, 0, 0);

        _updateButton = new Button
        {
            Dock = DockStyle.Fill,
            Text = "Atualizar Dados",
            Height = 34,
            BackColor = UiPalette.Accent,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _updateButton.FlatAppearance.BorderSize = 0;
        topLayout.Controls.Add(_updateButton, 1, 0);

        var selectedInfo = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Cidade selecionada, risco atual e última atualização são exibidos no painel superior.",
            ForeColor = UiPalette.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft
        };
        topLayout.Controls.Add(selectedInfo, 2, 0);

        var metrics = new GroupBox
        {
            Dock = DockStyle.Top,
            Height = 130,
            Text = "Indicadores atuais",
            ForeColor = UiPalette.TextSecondary,
            BackColor = UiPalette.Surface,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            Margin = new Padding(0, 12, 0, 0)
        };
        panel.Controls.Add(metrics);

        var metricLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            Padding = new Padding(12)
        };
        for (var i = 0; i < 5; i++)
        {
            metricLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        }
        metrics.Controls.Add(metricLayout);

        _temperatureValue = CreateMetricCell(metricLayout, 0, "Temperatura");
        _humidityValue = CreateMetricCell(metricLayout, 1, "Umidade");
        _rainValue = CreateMetricCell(metricLayout, 2, "Precipitação");
        _windValue = CreateMetricCell(metricLayout, 3, "Vento");
        _riskValue = CreateMetricCell(metricLayout, 4, "Nível de risco");

        var status = new GroupBox
        {
            Dock = DockStyle.Top,
            Height = 96,
            Text = "Status",
            ForeColor = UiPalette.TextSecondary,
            BackColor = UiPalette.Surface,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            Margin = new Padding(0, 12, 0, 0)
        };
        panel.Controls.Add(status);

        _lastUpdateValue = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiPalette.TextPrimary,
            Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold)
        };
        status.Controls.Add(_lastUpdateValue);

        return panel;
    }

    private Control BuildHistorySection()
    {
        var panel = BuildSectionPanel();

        _historyGrid = CreateGrid();
        _historyGrid.AutoGenerateColumns = false;
        _historyGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MonitoramentoRow.Data), HeaderText = "Data da consulta", Width = 160 });
        _historyGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MonitoramentoRow.Cidade), HeaderText = "Cidade", Width = 150 });
        _historyGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MonitoramentoRow.Temperatura), HeaderText = "Temperatura", Width = 120 });
        _historyGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MonitoramentoRow.Umidade), HeaderText = "Umidade", Width = 100 });
        _historyGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MonitoramentoRow.Precipitacao), HeaderText = "Chuva", Width = 100 });
        _historyGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MonitoramentoRow.Vento), HeaderText = "Vento", Width = 100 });
        _historyGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MonitoramentoRow.Eventos), HeaderText = "Eventos detectados", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        panel.Controls.Add(_historyGrid);
        return panel;
    }

    private Control BuildAlertsSection()
    {
        var panel = BuildSectionPanel();

        _alertsGrid = CreateGrid();
        _alertsGrid.AutoGenerateColumns = false;
        _alertsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AlertRow.Tipo), HeaderText = "Tipo do evento", Width = 180 });
        _alertsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AlertRow.Data), HeaderText = "Data", Width = 150 });
        _alertsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AlertRow.Severidade), HeaderText = "Severidade", Width = 120 });
        _alertsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AlertRow.Descricao), HeaderText = "Descrição", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        panel.Controls.Add(_alertsGrid);
        return panel;
    }

    private Control BuildReportsSection()
    {
        var panel = BuildSectionPanel();

        var summary = new GroupBox
        {
            Dock = DockStyle.Top,
            Height = 120,
            Text = "Resumo",
            ForeColor = UiPalette.TextSecondary,
            BackColor = UiPalette.Surface,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
        };
        panel.Controls.Add(summary);

        _reportSummaryLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Consultas: 0\r\nAlertas: 0\r\nMédia de temperatura: --\r\nMédia de precipitação: --",
            ForeColor = UiPalette.TextPrimary,
            Padding = new Padding(12),
            Font = new Font("Segoe UI", 10F)
        };
        summary.Controls.Add(_reportSummaryLabel);

        _reportTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            ReadOnly = true,
            Font = new Font("Consolas", 9.5F),
            BackColor = UiPalette.Background,
            ForeColor = UiPalette.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle
        };
        panel.Controls.Add(_reportTextBox);

        var exportButton = new Button
        {
            Dock = DockStyle.Bottom,
            Height = 38,
            Text = "Exportar TXT",
            ForeColor = UiPalette.TextPrimary,
            BackColor = UiPalette.Accent,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        exportButton.FlatAppearance.BorderSize = 0;
        exportButton.FlatAppearance.MouseOverBackColor = UiPalette.SurfaceLight;
        exportButton.FlatAppearance.MouseDownBackColor = UiPalette.Surface;
        exportButton.Click += (_, _) => ExportReport();
        panel.Controls.Add(exportButton);

        return panel;
    }

    private static Panel BuildSectionPanel()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0),
            BackColor = UiPalette.Background
        };
    }

    private static Label CreateMetricCell(TableLayoutPanel layout, int column, string title)
    {
        var cell = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = title,
            ForeColor = UiPalette.TextSecondary,
            BackColor = UiPalette.Surface,
            Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
        };

        var value = new Label
        {
            Dock = DockStyle.Fill,
            Text = "-",
            ForeColor = UiPalette.TextPrimary,
            Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };
        cell.Controls.Add(value);
        layout.Controls.Add(cell, column, 0);
        return value;
    }

    private DataGridView CreateGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            BackgroundColor = UiPalette.Background,
            BorderStyle = BorderStyle.FixedSingle,
            GridColor = UiPalette.Border,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            EnableHeadersVisualStyles = false
        };

        grid.ColumnHeadersDefaultCellStyle.BackColor = UiPalette.Sidebar;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = UiPalette.TextPrimary;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = UiPalette.Sidebar;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = UiPalette.TextPrimary;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);

        grid.DefaultCellStyle.BackColor = UiPalette.Background;
        grid.DefaultCellStyle.ForeColor = UiPalette.TextPrimary;
        grid.DefaultCellStyle.SelectionBackColor = UiPalette.SurfaceLight;
        grid.DefaultCellStyle.SelectionForeColor = UiPalette.TextPrimary;
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.25F);

        grid.AlternatingRowsDefaultCellStyle.BackColor = UiPalette.Surface;
        grid.AlternatingRowsDefaultCellStyle.ForeColor = UiPalette.TextPrimary;
        grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = UiPalette.SurfaceLight;
        grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = UiPalette.TextPrimary;

        return grid;
    }

    private void BindEvents()
    {
        _cityComboBox.SelectedIndexChanged += (_, _) => RefreshUi();
        _updateButton.Click += async (_, _) => await RefreshSelectedCityAsync();
    }

    private async Task EnsureInitialDataAsync()
    {
        if (_initialLoadAttempted)
        {
            return;
        }

        _initialLoadAttempted = true;

        if (_cityComboBox.SelectedIndex < 0)
        {
            _cityComboBox.SelectedIndex = 0;
        }

        await RefreshSelectedCityAsync(showMessage: false);
    }

    private async Task RefreshSelectedCityAsync(bool showMessage = true)
    {
        var city = SelectedCity;
        if (city is null)
        {
            return;
        }

        try
        {
            Cursor = Cursors.WaitCursor;

            var response = await _climateProvider.ObterDadosAsync(city.Coordenadas.Latitude, city.Coordenadas.Longitude);
            var current = response.Current ?? throw new ApiCommunicationException("A API não retornou dados climáticos atuais.");

            var climate = new DadoClimatico
            {
                Temperatura = current.Temperature2M,
                Umidade = current.RelativeHumidity2M,
                Precipitacao = current.Precipitation,
                VelocidadeVento = current.WindSpeed10M,
                DataHora = current.Time == default ? DateTime.Now : current.Time
            };

            city.DadoClimatico = climate;

            var analysis = _analysisService.Analisar(climate, city);

            _history.Add(new MonitoramentoRegistro
            {
                Cidade = city,
                DadoClimatico = climate,
                DataConsulta = DateTime.Now,
                Eventos = analysis.Eventos
            });
            _alerts.AddRange(analysis.Alertas);

            RefreshUi();

            if (showMessage)
            {
                MessageBox.Show(
                    $"Dados atualizados para {city.Nome}.",
                    "Climate Sentinel",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (ApiCommunicationException ex)
        {
            MessageBox.Show(
                $"{ex.Message}\r\n\r\nA aplicação continuará aberta.",
                "Falha na API",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        catch (AnaliseClimaticaException ex)
        {
            MessageBox.Show(
                $"{ex.Message}\r\n\r\nA aplicação continuará aberta.",
                "Falha na análise",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erro inesperado: {ex.Message}\r\n\r\nA aplicação continuará aberta.",
                "Climate Sentinel",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void RefreshUi()
    {
        var city = SelectedCity;
        if (city is null)
        {
            return;
        }

        var latest = GetLatestRecord(city);
        var state = GetRiskState(latest);

        _selectedCityLabel.Text = city.ToString();
        _lastUpdateValue.Text = latest is null ? "Sem leituras" : latest.DataConsulta.ToString("dd/MM/yyyy HH:mm");
        _temperatureValue.Text = latest is null ? "-" : $"{latest.DadoClimatico.Temperatura:F1} °C";
        _humidityValue.Text = latest is null ? "-" : $"{latest.DadoClimatico.Umidade:F0}%";
        _rainValue.Text = latest is null ? "-" : $"{latest.DadoClimatico.Precipitacao:F1} mm";
        _windValue.Text = latest is null ? "-" : $"{latest.DadoClimatico.VelocidadeVento:F1} km/h";
        _riskValue.Text = state.Label;

        _historyGrid.DataSource = BuildHistoryRows();
        _alertsGrid.DataSource = BuildAlertRows();
        _reportSummaryLabel.Text = BuildReportSummary();
        _reportTextBox.Text = BuildReportText();
    }

    private List<MonitoramentoRow> BuildHistoryRows()
    {
        return _history
            .OrderByDescending(item => item.DataConsulta)
            .Select(item => new MonitoramentoRow(
                item.DataConsulta.ToString("dd/MM/yyyy HH:mm"),
                item.Cidade.Nome,
                $"{item.DadoClimatico.Temperatura:F1} °C",
                $"{item.DadoClimatico.Umidade:F0}%",
                $"{item.DadoClimatico.Precipitacao:F1} mm",
                $"{item.DadoClimatico.VelocidadeVento:F1} km/h",
                item.Eventos.Count == 0 ? "Nenhum" : string.Join(", ", item.Eventos.Select(e => e.Nome))))
            .ToList();
    }

    private List<AlertRow> BuildAlertRows()
    {
        return _alerts
            .OrderByDescending(item => item.DataCriacao)
            .Select(item => new AlertRow(item.TipoEvento, item.DataCriacao.ToString("dd/MM/yyyy HH:mm"), item.Severidade, item.Descricao))
            .ToList();
    }

    private string BuildReportSummary()
    {
        var report = CreateReport();
        var avgTemp = report.TotalConsultas == 0 ? "--" : $"{report.MediaTemperatura:F1} °C";
        var avgRain = report.TotalConsultas == 0 ? "--" : $"{report.MediaPrecipitacao:F1} mm";

        return $"Consultas: {report.TotalConsultas}\r\nAlertas: {report.TotalAlertas}\r\nMédia de temperatura: {avgTemp}\r\nMédia de precipitação: {avgRain}";
    }

    private string BuildReportText() => CreateReport().GerarTexto();

    private void ExportReport()
    {
        try
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Arquivo TXT (*.txt)|*.txt",
                FileName = $"relatorio-climatico-{DateTime.Now:yyyyMMdd-HHmm}.txt"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            CreateReport().SalvarEmArquivo(dialog.FileName);
            MessageBox.Show("Relatório exportado com sucesso.", "Climate Sentinel", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Não foi possível exportar o relatório: {ex.Message}", "Climate Sentinel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static RiskState GetRiskState(MonitoramentoRegistro? record)
    {
        if (record is null)
        {
            return new RiskState("Aguardando leitura", UiPalette.Accent);
        }

        return record.Eventos.Count switch
        {
            0 => new RiskState("Seguro", UiPalette.Safe),
            1 => new RiskState("Atenção", UiPalette.Attention),
            2 => new RiskState("Alto risco", UiPalette.HighRisk),
            _ => new RiskState("Crítico", UiPalette.Critical)
        };
    }

    private RelatorioClimatico CreateReport() => new(_history, _alerts);

    private CidadeMonitorada? SelectedCity =>
        _cityComboBox.SelectedIndex >= 0 && _cityComboBox.SelectedIndex < _cities.Count
            ? _cities[_cityComboBox.SelectedIndex]
            : null;

    private MonitoramentoRegistro? GetLatestRecord(CidadeMonitorada city) =>
        _history.Where(item => item.Cidade.Id == city.Id).OrderByDescending(item => item.DataConsulta).FirstOrDefault();

    private sealed record RiskState(string Label, Color Color);

    private sealed record MonitoramentoRow(
        string Data,
        string Cidade,
        string Temperatura,
        string Umidade,
        string Precipitacao,
        string Vento,
        string Eventos);

    private sealed record AlertRow(
        string Tipo,
        string Data,
        string Severidade,
        string Descricao);
}
