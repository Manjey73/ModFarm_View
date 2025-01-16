
using Scada.Forms;
using Scada.Forms.Forms;
using System.Reflection;
using System.Xml.Linq;

namespace Scada.Server.Modules.ModFarm.View.Forms
{
    public partial class FrmParameters : Form
    {
        #region Variables
        //public FrmModuleConfig frmParentGloabal;      // global general form
        public bool boolParent = false;                 // сhild startup flag
        private BindingSource bsAttribute = new BindingSource();
        private BindingSource bsCData = new BindingSource();
        Dictionary<string, object> tags;
        private IEnumerable<XAttribute> attr;
        private IEnumerable<XText> xtext;
        private IEnumerable<XCData> xcdata;
        private IEnumerable<TreeNode> query;
        Size dgvSize;
        Size rText;
        private int positionY = 10;
        int rowIndex = -1;
        ToolStrip toolStrip;
        ToolStripButton findCnl;

        // Интерфейс
        private List<string> YesNo = new List<string> { "true", "false" }; // fill the drop down items.. Выпадающий список для ячейки с булевой переменной 
        private List<string> listOfVar;
        // Список задач, которые требуется исключить из списка выбора. Должны быть доступны только те, программы которых можно выполнить.
        private List<string> ignoreTask = new List<string> { "Farm", "Func", "TON", "FrmModuleConfig", "FrmParameters", "Device", "Programs", "Calendar", "ProgramX", "DayX", "Input", "Output", "TimeSpanExtender", "ProgramzAttribute" };
        private List<string> listOfTask;
        #endregion Variables

        public FrmParameters(ToolStrip _toolStrip) // С кнопкой работает ToolStripButton Search
        {
            InitializeComponent();

            rtbParam.TextChanged -= TxtParam_TextChanged; // TEST
            dgvAttribute.CellValueChanged -= DgvAttribute_CellValueChanged;
            dgvCData.CellValueChanged -= DgvCData_CellValueChanged;


            FormatWindow();

            rtbNode.ContentsResized += RtbNode_ContentsResized;

            toolStrip = _toolStrip;
            findCnl = toolStrip.Items.OfType<ToolStripButton>().Where(b => b.Name == "tbFind").FirstOrDefault();
            findCnl.Click += Search_Click;
        }

        private void FormatWindow() // bool hasParent
        {
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Left | DockStyle.Top;
            TopLevel = false;
        }

        private void FrmParameters_Load(object sender, EventArgs e)
        {
            // translate the form
            FormTranslator.Translate(this, GetType().FullName);

            // translate menu
            //FormTranslator.Translate(cmnuLstVariable, GetType().FullName);

            ConfigToControls();

            // translate the listview
            //FormTranslator.Translate(lstVariables, GetType().FullName);
            Modified = frmParentGloabal.modified;
        }

        private void ConfigToControls()
        {
            rtbParam.Visible = false;

            dgvAttribute.Visible = false;
            dgvCData.Visible = false;
            //txtParam.Size = new Size(580 - txtParam.Location.X, txtParam.Width);

            findCnl.Enabled = false;

            var currentNode = ModFarmView.fConfig.trvRoot.SelectedNode;
            var currentNodes = ModFarmView.fConfig.trvRoot.SelectedNode.Nodes;

            if (currentNode.Tag is Dictionary<string, object>)
            {
                tags = currentNode.Tag as Dictionary<string, object>;
                var count = 0;
                var dgvRH = 0;

                if (tags != null && tags.ContainsKey("Text"))
                {
                    xtext = tags["Text"] as IEnumerable<XText>;

                    if (xtext != null && xtext.Any())
                    {
                        if (tags.ContainsKey("Attributes"))
                        {
                            rtbParam.Location = new Point(rtbParam.Location.X, positionY);

                            int rtbSize = rtbParam.Size.Height + positionY;
                            dgvAttribute.Location = new Point(12, rtbSize + 10); // TEST

                            positionY = rtbSize + 10;
                        }

                        rtbParam.Visible = true; // TEST в richTextBox

                        XText txt = xtext.FirstOrDefault();

                        if (xtext is not XCData)
                        {
                            //result = ModFarmView.fConfig.trvRoot.SelectedNode.Nodes.OfType<TreeNode>()
                            //        .FirstOrDefault(node => node.Text.Equals("#text"));
                            //lbText.Text = result.Text;

                            rtbParam.Text = txt.Value; // TEST в richTextBox
                        }
                    }
                }
                else
                {
                    dgvAttribute.Location = new Point(12, 10); // TEST
                }

                if (tags != null && tags.ContainsKey("Attributes")) // Тест аттрибутов на null
                {
                    attr = tags["Attributes"] as IEnumerable<XAttribute>;

                    count = attr.Count();

                    bsAttribute.DataSource = attr;
                    dgvAttribute.DataSource = bsAttribute;

                    dgvRH = dgvAttribute.Rows[0].Height;

                    dgvAttribute.Size = new Size(588, (count + 1) * dgvRH);

                    gbComment.Location = new Point(12, (count + 1) * dgvRH + positionY + 10);     // Расположение GroupBox при наличии Атрибутов

                    dgvAttribute.AllowUserToResizeRows = false;                         // Запретить изменять размер строк
                    dgvAttribute.Visible = true;
                    dgvAttribute.RowHeadersVisible = false;
                    dgvAttribute.Columns["IsNamespaceDeclaration"].Visible = false;     // Скрыть столбец со служебными переменными
                    dgvAttribute.Columns["NextAttribute"].Visible = false;              // Скрыть столбец со служебными переменными
                    dgvAttribute.Columns["NodeType"].Visible = false;                   // Скрыть столбец со служебными переменными
                    dgvAttribute.Columns["PreviousAttribute"].Visible = false;          // Скрыть столбец со служебными переменными
                    dgvAttribute.Columns["BaseUri"].Visible = false;                    // Скрыть столбец со служебными переменными
                    dgvAttribute.Columns["Document"].Visible = false;                   // Скрыть столбец со служебными переменными
                    dgvAttribute.Columns["Parent"].Visible = false;                     // Скрыть столбец со служебными переменными
                    dgvAttribute.Columns["Name"].HeaderText = ModulePhrases.dgvAttrName;
                    dgvAttribute.Columns["Value"].HeaderText = ModulePhrases.dgvAttrValue;

                    dgvSize = dgvAttribute.Size;

                    dgvAttribute.Columns["Name"].Width = dgvSize.Width / 3;
                    dgvAttribute.Columns["Value"].Width = dgvSize.Width - (dgvSize.Width / 3) - 18;

                    //События для DataGridView
                    dgvAttribute.CellBeginEdit += DgvAttribute_CellBeginEdit;
                    dgvAttribute.DataError += new DataGridViewDataErrorEventHandler(dgvAttribute_DataError);
                }

                List<TreeNode> listquery = currentNodes.Cast<TreeNode>().Where(node => node.Text.Contains("#comment")).ToList();

                if (listquery != null && listquery.Count > 0)
                {
                    var cnt = listquery.Count;
                    foreach (TreeNode node in listquery)
                    {
                        rtbNode.Text += $"{((XComment)node.Tag).Value}"; // TEST Новая линия при выводе комментариев
                        if (cnt > 1) rtbNode.Text += Environment.NewLine;
                        cnt--;
                    }
                    gbComment.Size = new Size(568, rText.Height + 36);
                }
                else
                    gbComment.Visible = false;

                if (tags != null && tags.ContainsKey("CData"))
                {
                    xcdata = tags["CData"] as IEnumerable<XCData>;

                    if (xcdata != null && xcdata.Any())
                    {
                        bsCData.DataSource = xcdata;
                        dgvCData.DataSource = bsCData;

                        dgvCData.Location = new Point(12, positionY + gbComment.Height + 88);     // Расположение CDATA при наличии Атрибутов positionY + 68 +

                        dgvCData.Visible = true;
                        dgvCData.RowHeadersVisible = false;
                        dgvCData.ColumnHeadersVisible = false;
                        dgvCData.Columns["NextNode"].Visible = false;               // Скрыть столбец со служебными переменными
                        dgvCData.Columns["PreviousNode"].Visible = false;           // Скрыть столбец со служебными переменными
                        dgvCData.Columns["BaseUri"].Visible = false;                // Скрыть столбец со служебными переменными
                        dgvCData.Columns["Document"].Visible = false;               // Скрыть столбец со служебными переменными
                        dgvCData.Columns["Parent"].Visible = false;                 // Скрыть столбец со служебными переменными
                        dgvCData.Columns["NodeType"].Width = dgvSize.Width / 4;
                        dgvCData.Columns["Value"].Width = dgvSize.Width - (dgvSize.Width / 4) - 22;

                        dgvCData.Size = new Size(588, xcdata.Count() * dgvCData.Rows[0].Height + 3);
                    }
                }
            }
            else if (currentNode.Tag is string && currentNode.Tag is not XCData) // Зачем комментарий первого атрибута в текстовой метке ?
            {
                gbComment.Visible = false;
                rtbParam.Visible = true;
                rtbParam.Text = currentNode.Tag.ToString();
            }
            else if (currentNode.Tag is XComment)
            {
                gbComment.Visible = false;
                if (currentNode.Text == "#comment") // Для комментариев используем RichTextBox
                {
                    rtbParam.Visible = true;
                    rtbParam.Text = ((XComment)currentNode.Tag).Value;
                }
            }
            else if (currentNode.Tag is XCData)
            {
                gbComment.Visible = false;
                if (currentNode.Text == "#cdata-section")
                {
                    gbComment.Visible = false;

                    rtbParam.Visible = true;
                    rtbParam.Text = ((XCData)currentNode.Tag).Value;
                }
            }
            else if (currentNode.Tag is XAttribute)
            {
                XAttribute attribute = (XAttribute)currentNode.Tag;
                if (currentNode.Parent.Tag is Dictionary<string, object>)
                {
                    Dictionary<string, object> dict = currentNode.Parent.Tag as Dictionary<string, object>;

                    if (dict.ContainsKey("Comment"))
                    {
                        IEnumerable<XComment> comm = dict["Comment"] as IEnumerable<XComment>;
                        IEnumerable<XComment> xcomm = comm.Where(x => x.Value.StartsWith(attribute.Name.ToString()));

                        if (xcomm.Any())
                        {
                            if (xcomm.FirstOrDefault() != null)
                                rtbNode.Text = xcomm.FirstOrDefault().Value; // .Trim([(char)0xD0, (char)0x0A])

                            gbComment.Size = new Size(568, rText.Height + 36);      // Размер GroupBox - определить по количеству строк RichTextBox - собственно по queryComment.Count
                        }
                        else
                            gbComment.Visible = false;
                    }
                    else
                        gbComment.Visible = false;
                }
                rtbParam.Visible = true;
                rtbParam.Text = attribute.Value;
            }

            rowIndex = -1;

            try
            {
                DataGridViewRow row = dgvAttribute.Rows.Cast<DataGridViewRow>()
                     .Where(r => r.Cells["Name"].Value.ToString().Equals(ModulePhrases.CnlNum.Replace(" ", ""))) // Equals("CnlNum")
                     .First();
                rowIndex = row.Index;
            }
            catch { }

            if (rowIndex > -1)
            {
                findCnl.Enabled = true;
            }

            rtbParam.TextChanged += TxtParam_TextChanged;
            dgvAttribute.CellValueChanged += DgvAttribute_CellValueChanged;
            dgvCData.CellValueChanged += DgvCData_CellValueChanged;
        }

        #region ChangeToCellBox 
        private void DgvAttribute_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            var dgvPrgName = dgvAttribute.Columns["Name"];
            int idClmName = -1;
            if (dgvPrgName != null) // Проверка что такое поле есть, соответственно есть и поле Value
            {
                idClmName = dgvAttribute.Columns.IndexOf(dgvPrgName);
                string valClm = dgvAttribute[idClmName, e.RowIndex].Value.ToString();

                if (valClm == ModulePhrases.Active.Replace(" ", "") || valClm == ModulePhrases.ProgramActive.Replace(" ", "") || valClm == ModulePhrases.Retain.Replace(" ", "") ||
                    valClm == ModulePhrases.Initialize.Replace(" ", "") || valClm == ModulePhrases.Cooling.Replace(" ", "") || valClm == ModulePhrases.ProgramRestart.Replace(" ", "") ||
                    valClm == ModulePhrases.VisibleLogPrgFile.Replace(" ", ""))
                {
                    DataGridViewComboBoxCell c = new DataGridViewComboBoxCell();
                    c.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing; // убрать серый стиль ComboBoxCell, приводит к глюку лишних нажатий мыши
                    c.Style.BackColor = Color.White;
                    c.DataSource = YesNo;
                    dgvAttribute[e.ColumnIndex, e.RowIndex] = c;  // change the cell
                }
                else if (valClm == ModulePhrases.Task)
                {
                    //frmParentGloabal.rtb_Log.Text += $"{ModulePhrases.Task} : Получить список задач" + Environment.NewLine;

                    Type[] typelist = Assembly.GetExecutingAssembly().GetExportedTypes(); // Получение списка только интересующего класса

                    listOfTask = new List<string> { "" };
                    // Отключить из добавления в список внутренних классов
                    foreach (Type type in typelist)
                    {
                        int idT = -1;
                        idT = noTask.IndexOf(type.Name);
                        if (idT == -1) listOfTask.Add(type.Name);
                    }

                    var asmlist = Assembly.Load("ModFarmPlc").GetExportedTypes();
                    foreach (var asmtype in asmlist)
                    {
                        //frmParentGloabal.rtb_Log.Text += $"Assembly task: {asmtype.Name}" + Environment.NewLine; // TEST
                        int idT = -1;
                        idT = noTask.IndexOf(asmtype.Name);
                        if (idT == -1) listOfTask.Add(asmtype.Name);
                    }


                    var cur = dgvAttribute[e.ColumnIndex, e.RowIndex].Value as string;
                    if (listOfTask != null)
                    {
                        var ix = listOfTask.IndexOf(cur);
                        if (ix == -1 && cur != null)
                        {
                            dgvAttribute[e.ColumnIndex, e.RowIndex].Value = "";
                        }
                        DataGridViewComboBoxCell lT = new DataGridViewComboBoxCell();
                        lT.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing; // убрать серый стиль ComboBoxCell, приводит к глюку лишних нажатий мыши
                        lT.Style.BackColor = Color.White;
                        lT.DataSource = listOfTask;
                        dgvAttribute[e.ColumnIndex, e.RowIndex] = lT;  // change the cell
                    }
                }
                else if (valClm == ModulePhrases.ValName)
                {
                    // Тут надо определить имя задачи Parent.Parent  - Дедушка GrandFather
                    Dictionary<string, object> grandFather = ModFarmView.fConfig.trvRoot.SelectedNode.Parent.Parent.Tag as Dictionary<string, object>;

                    if (grandFather != null && grandFather.ContainsKey("Attributes"))
                    {
                        List<XAttribute> lattr = grandFather["Attributes"] as List<XAttribute>;
                        string taskName = lattr.Where(x => x.Name == ModulePhrases.Task).FirstOrDefault().Value.ToString();

                        if (taskName != null)
                        {
                            // Тут обращаемся к получению списка переменных задачи для добавления в список listOfVar
                            listOfVar = GetTaskValName(taskName);
                            // Как проверить, что ячейка содержит значение из списка и если нет, обнулить перед переводов в ComboBox?
                            var cur = dgvAttribute[e.ColumnIndex, e.RowIndex].Value as string;

                            if (listOfVar != null)
                            {
                                var ix = listOfVar.IndexOf(cur);
                                if (ix == -1 && cur != null)
                                {
                                    dgvAttribute[e.ColumnIndex, e.RowIndex].Value = "";
                                }
                                DataGridViewComboBoxCell l = new DataGridViewComboBoxCell();
                                l.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing; // убрать серый стиль ComboBoxCell, приводит к глюку лишних нажатий мыши
                                l.Style.BackColor = Color.White;
                                l.DataSource = listOfVar;
                                dgvAttribute[e.ColumnIndex, e.RowIndex] = l;  // change the cell
                            }
                        }
                    }
                }
            }
        }

        private void dgvAttribute_DataError(object sender, DataGridViewDataErrorEventArgs anErr) // Если ошиблись с вводом параметра то в случае отсутствия будет пустая строка
        {
            string cval = dgvAttribute[anErr.ColumnIndex, anErr.RowIndex].Value.ToString(); // .ToLower()
            dgvAttribute[anErr.ColumnIndex, anErr.RowIndex].Value = "";
            anErr.ThrowException = false;
        }
        #endregion ChangeToCellBox

        private List<string> GetTaskValName(string task) // List<XAttribute> lattr
        {
            List<string> listVal = new List<string>();

            Type typelist = Assembly.GetExecutingAssembly().GetExportedTypes().Where(x => x.Name == task).FirstOrDefault(); // Получение списка только интересующего класса
            Type type; // = Assembly.GetExecutingAssembly().GetType(typelist.FullName); //  "Scada.Server.Modules.ModFarm.Logic.Heater"


            if (typelist == null)
            {
                //frmParentGloabal.rtb_Log.Text += $"Нет такой задачи в основном коде:   {task}" + Environment.NewLine; // TEST

                typelist = Assembly.Load("ModFarmPlc").GetExportedTypes().Where(x => x.Name == task).FirstOrDefault();
                if (typelist == null)
                {
                    //frmParentGloabal.rtb_Log.Text += $"Нет такой задачи в дополнительном коде:   {task}" + Environment.NewLine; // TEST
                    return null;
                }
                else
                {
                    type = typelist;
                }
            }
            else
            {
                type = typelist;
                //type = Assembly.GetExecutingAssembly().GetType(typelist.FullName); //  "Scada.Server.Modules.ModFarm.Logic.Heater" // TEST
            }

            object instance = Activator.CreateInstance(type);

            // Добавляем имена переменных в список - Как сделать проверку уже добавленных в параметры входов, выходов имен и удалять из списка для нового добавления
            // и обратно возвращать, если имя удалили ?
            listVal.Add(""); // Добавляем пустое поле
            foreach (FieldInfo mi in instance.GetType().GetFields())
            {
                // Добавляем все, кроме "terminated" для использования.
                if (mi.Name != "terminated")
                    listVal.Add(mi.Name);
            }
            return listVal;
        }

        private void RtbNode_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            RichTextBox richTextBox = (RichTextBox)sender;
            richTextBox.Width = 556;
            richTextBox.Height = e.NewRectangle.Height;
            rText = new Size(556, e.NewRectangle.Height);
        }

        #region Form Control
        /// <summary>
        /// Gets or sets a value indicating whether the configuration was modified.
        /// </summary>
        private bool Modified
        {
            get
            {
                return frmParentGloabal.modified;
            }
            set
            {
                frmParentGloabal.modified = value; // modified = value;
                frmParentGloabal.btOK.Enabled = frmParentGloabal.modified;
            }
        }
        #endregion Form Control

        private void Search_Click(object sender, EventArgs e)
        {
            if (frmParentGloabal.ConfigDataset != null)
            {
                string num = "";

                rowIndex = -1;
                try
                {
                    DataGridViewRow row = dgvAttribute.Rows.Cast<DataGridViewRow>()
                         .Where(r => r.Cells["Name"].Value.ToString().Equals(ModulePhrases.CnlNum.Replace(" ", ""))) //  "CnlNum"
                         .First();
                    rowIndex = row.Index;
                }
                catch{}

                if (rowIndex > -1)
                {
                    num = dgvAttribute.Rows[rowIndex].Cells["Value"].Value.ToString();
                    int initCnl = 0;
                    bool isNum = int.TryParse(num, out initCnl);

                    FrmCnlSelect frmCnlSelect = new FrmCnlSelect(frmParentGloabal.ConfigDataset)
                    {
                        MultiSelect = false,
                        SelectedCnlNum = initCnl
                    };
                    if (frmCnlSelect.ShowDialog() == DialogResult.OK)
                    {
                        dgvAttribute.Rows[rowIndex].Cells["Value"].Value = frmCnlSelect.SelectedCnlNum.ToString(); // Было [3] вместо rowindex
                    }
                }
            }
            //else
            //{
            //    //frmParentGloabal.rtb_Log.Text += $"Нет данных" + Environment.NewLine; // TEST
            //}
        }

        #region TxtParam_TextChanged
        private void SaveTextToTagDict(string boxText)
        {
            List<XText> text = (tags["Text"] as IEnumerable<XText>).ToList();

            XText xtext = text.FirstOrDefault();

            xtext.Value = boxText; // Строка 
            List<XText> lXtext = new List<XText>();
            lXtext.Add(xtext);

            IEnumerable<XText> ieXtext = lXtext.AsEnumerable<XText>();
            tags["Text"] = ieXtext;
        }

        private void TxtParam_TextChanged(object sender, EventArgs e)
        {
            string boxText = "";

            try
            {
                TextBox tb = (TextBox)sender;
                boxText = tb.Text;
            }
            catch
            {
                try
                {
                    RichTextBox rtb = (RichTextBox)sender;
                    boxText = rtb.Text;
                }
                catch{}
            }

            TreeNode treeNode = null;
            if (ModFarmView.fConfig.trvRoot.SelectedNode.Tag is Dictionary<string, object>)
            {
                tags = ModFarmView.fConfig.trvRoot.SelectedNode.Tag as Dictionary<string, object>;
                TreeNode result = null;

                result = ModFarmView.fConfig.trvRoot.SelectedNode.Nodes.OfType<TreeNode>()
                    .FirstOrDefault(node => node.Text.Equals("#text"));

                if (result != null)
                {
                    ModFarmView.fConfig.trvRoot.SelectedNode.Nodes[result.Index].Tag = boxText; // Запись текста в ноду #text
                    SaveTextToTagDict(boxText);    // Необходимо еще записать в себя, в свой словарь текста
                }
            }
            else if (ModFarmView.fConfig.trvRoot.SelectedNode.Tag is string) // Изменение строки если редактируем саму строку
            {
                ModFarmView.fConfig.trvRoot.SelectedNode.Tag = boxText;

                treeNode = ModFarmView.fConfig.trvRoot.SelectedNode.Parent; // perentNode

                if (treeNode != null)
                {
                    if (treeNode.Tag is Dictionary<string, object>)
                    {
                        tags = treeNode.Tag as Dictionary<string, object>;
                        if (tags.ContainsKey("Text") && ModFarmView.fConfig.trvRoot.SelectedNode.Text == "#text")
                        {
                            SaveTextToTagDict(boxText);
                        }
                    }
                }
            }
            else if (ModFarmView.fConfig.trvRoot.SelectedNode.Tag is XComment)
            {
                ModFarmView.fConfig.trvRoot.SelectedNode.Tag = new XComment(boxText);

                treeNode = ModFarmView.fConfig.trvRoot.SelectedNode.Parent; // perentNode

                if (treeNode != null)
                {
                    if (treeNode.Tag is Dictionary<string, object>)
                    {
                        tags = treeNode.Tag as Dictionary<string, object>;
                        if (tags.ContainsKey("Comment") && ModFarmView.fConfig.trvRoot.SelectedNode.Text == "#comment")
                        {
                            query = treeNode.Nodes.Cast<TreeNode>().Where(node => node.Text.Contains("#comment"));
                            List<XComment> lXComment = new List<XComment>();

                            foreach (TreeNode comment in query)
                            {
                                XComment xcomment = new XComment(((XComment)comment.Tag).Value);
                                lXComment.Add(xcomment);
                            }
                            IEnumerable<XComment> ieXcomment = lXComment.AsEnumerable<XComment>();
                            tags["Comment"] = ieXcomment;
                        }
                    }
                }
            }
            else if (ModFarmView.fConfig.trvRoot.SelectedNode.Tag is XCData)
            {
                ModFarmView.fConfig.trvRoot.SelectedNode.Tag = new XCData(boxText);
                treeNode = ModFarmView.fConfig.trvRoot.SelectedNode.Parent;  // perentNode

                if (treeNode != null)
                {
                    if (treeNode.Tag is Dictionary<string, object>)
                    {
                        tags = treeNode.Tag as Dictionary<string, object>;
                        if (tags.ContainsKey("CData") && ModFarmView.fConfig.trvRoot.SelectedNode.Text == "#cdata-section")
                        {
                            query = treeNode.Nodes.Cast<TreeNode>().Where(node => node.Text.Contains("#cdata-section"));
                            List<XCData> lCdata = new List<XCData>();

                            foreach (TreeNode cdata in query)
                            {
                                XCData xdata = new XCData(((XCData)cdata.Tag).Value);
                                lCdata.Add(xdata);
                            }
                            IEnumerable<XCData> ieCData = lCdata.AsEnumerable<XCData>();
                            tags["CData"] = ieCData;
                        }
                    }
                }
            }
            else if (ModFarmView.fConfig.trvRoot.SelectedNode.Tag is XAttribute)
            {
                XAttribute attribute = (XAttribute)ModFarmView.fConfig.trvRoot.SelectedNode.Tag;
                attribute.Value = boxText;

                treeNode = ModFarmView.fConfig.trvRoot.SelectedNode.Parent; // perentNode

                if (treeNode != null)
                {
                    if (treeNode.Tag is Dictionary<string, object>)
                    {
                        tags = treeNode.Tag as Dictionary<string, object>;
                        if (tags.ContainsKey("Attributes") && ModFarmView.fConfig.trvRoot.SelectedNode.Tag is XAttribute)
                        {
                            query = treeNode.Nodes.Cast<TreeNode>().Where(node => node.Tag is XAttribute);
                            List<XAttribute> lAttribute = new List<XAttribute>();

                            foreach (var attr in query)
                            {
                                XAttribute xattr = new XAttribute(attr.Text, ((XAttribute)attr.Tag).Value); //  attr.Tag
                                lAttribute.Add(xattr);
                            }
                            IEnumerable<XAttribute> ieAttribute = lAttribute.AsEnumerable<XAttribute>();
                            tags["Attributes"] = ieAttribute;
                        }
                    }
                }
            }
            Modified = true;
        }
        #endregion TxtParam_TextChanged

        #region DgvAttribute_CellValueChanged
        private void DgvAttribute_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int ri = e.RowIndex;

            if (ri != -1)
            {
                int ciName = dgvAttribute.Columns["Name"].Index;
                int ciValue = dgvAttribute.Columns["Value"].Index;

                TreeNode result = ModFarmView.fConfig.trvRoot.SelectedNode.Nodes.OfType<TreeNode>()
                            .FirstOrDefault(node => node.Text.Equals(dgvAttribute[ciName, ri].Value.ToString()));

                if (result.Tag is XAttribute)
                {
                    ModFarmView.fConfig.trvRoot.SelectedNode.Nodes[result.Index].Tag = new XAttribute(ModFarmView.fConfig.trvRoot.SelectedNode.Nodes[result.Index].Text, ModFarmView.fConfig.trvRoot.SelectedNode.Nodes[result.Index].Tag = dgvAttribute[ciValue, ri].Value);
                }
                //else if (result.Tag is string)
                //{
                //    ModFarmView.fConfig.trvRoot.SelectedNode.Nodes[result.Index].Tag = dgvAttribute[ciValue, ri].Value;
                //}
                Modified = true;
            }
        }
        #endregion DgvAttribute_CellValueChanged

        #region DgvCData_CellValueChanged
        private void DgvCData_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int ri = e.RowIndex;
            TreeNode current = ModFarmView.fConfig.trvRoot.SelectedNode; // Данные выбранной Ноды

            if (ri != -1)
            {
                List<TreeNode> lNode = current.Nodes.OfType<TreeNode>().Where(x => x.Text == "#cdata-section").ToList(); // Список нод cdata-section для определения индекса редактируемой Ноды
                int ciName = dgvCData.Columns["NodeType"].Index;
                int ciValue = dgvCData.Columns["Value"].Index;
                current.Nodes[lNode[ri].Index].Tag = new XCData(dgvCData[ciValue, ri].Value.ToString()); // Изменение данных ноды cdata-section при редактировании данных в DataGreedView
                Modified = true; // TEST
            }
        }
        #endregion DgvCData_CellValueChanged

    }
}
