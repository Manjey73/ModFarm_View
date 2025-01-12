using Scada.Data.Models;
using Scada.Server.Modules.ModFarm.Config;
using System.Xml.Linq;
using System.Xml;
using Scada.Forms;
using System.Reflection;
using System.Xml.Serialization;
using Scada.Lang;
using Scada.Server.Lang;

namespace Scada.Server.Modules.ModFarm.View.Forms
{
    public partial class FrmModuleConfig : Form
    {
        #region Variables
        FrmParameters frmParameters; // child form properties Form();
        public Dictionary<string, object> tags;     // Словарь для сохранения параметров в родительском Ноде
        private string filePath;

        private readonly string configFileName;         // the configuration file name
        private string shortFileName = "";
        private string fileName;
        public bool modified;                           // indicates that the module configuration is modified
        private readonly AppDirs appDirs;

        private Dictionary<string, string> xml_node = new Dictionary<string, string>();

        private ContextMenuStrip contextMenu; // TEST

        private ToolStripMenuItem submenuGroup = new ToolStripMenuItem();
        private Dictionary<string, string> CurLang = new Dictionary<string, string>();
        public static Dictionary<string, string> PrevLang = new Dictionary<string, string>();


        // Словарь индексов изображений для интерфейса
        public static Dictionary<string, int> imageIndex = new Dictionary<string, int> // Словарь индексов изображений
        {
            { "Chickens", 5 }, { "Gooses", 12 }, { "Turkeys", 9 }, { "Quails", 13 }, { "Ducks", 7 }, { "Ostriches", 15 },
            { "Cows", 8 }, { "Sheeps", 10 }, { "Goats", 14 }, { "Pigs", 6 }, { "Rabbits", 11 }, { "Others", 16 },
            { "Calendar", 20 }, { "Day", 21 }, { "Device", 2 }, { "Programs", 22 }, { "Program", 19 },
            { "Inputs", 17 }, { "Input", 17 }, { "Outputs", 18 }, { "Output", 18 }
        };

        #endregion Variables

        public FrmModuleConfig()
        {
            ModFarmView.fConfig = this; // Присвоить ссылку на себя для доступа к переменным формы.
            Text = ModulePhrases.AppTitle;

            InitializeComponent();
            ConfigDataset = null;

            // Создать субменю для Group
            SubMenuItem();
            // Создать Menu
            CreateMenu();
        }

        /// <summary>
        /// Gets or sets the configuration database.
        /// </summary>
        public ConfigDataset ConfigDataset { get; set; }

        public FrmModuleConfig(ConfigDataset configDataset, AppDirs appDirs)
            : this()
        {
            this.appDirs = appDirs ?? throw new ArgumentNullException(nameof(appDirs));
            configFileName = Path.Combine(appDirs.ConfigDir, ModuleConfig.DefaultFileName);
            modified = false;

            ArgumentNullException.ThrowIfNull(configDataset, nameof(configDataset));
            ConfigDataset = configDataset;
        }

        #region LoadXmlToTreeView  - Рабочий вариант
        private void LoadXmlToTreeView(XElement rootElement, TreeNodeCollection treeNodeCollection)
        {
            foreach (XElement xelement in rootElement.Elements())
            {
                int indexImage = 0;

                IEnumerable<XElement> elements = xelement.Elements();
                //Determine whether the element is a leaf element, that is, whether there are child elements below
                //If there are child elements, only add the element name, if it is a leaf element, add the element name and element content
                XName xname = xelement.Name;

                if (elements.Count() == 0) // Returns the number of elements in the incoming collection  // ReturnNumber(elements) == 0
                {
                    // Интерфейс - Иконки Нод TreeView
                    ReadXName(xelement, out indexImage, out xname);
                    // Интерфейс - Иконки Нод TreeView

                    TreeNode xnode = new TreeNode { Text = xname.ToString(), ImageIndex = indexImage, SelectedImageIndex = indexImage };
                    treeNodeCollection.Add(xnode);

                    tags = new Dictionary<string, object>();

                    ReadParameters(xelement, xnode);

                    xnode.Tag = tags;
                }
                else
                {
                    // Интерфейс - Иконки, Имена Нод TreeView
                    ReadXName(xelement, out indexImage, out xname);
                    // Интерфейс - Иконки, Имена Нод TreeView

                    TreeNode xnode = new TreeNode { Text = xname.ToString(), ImageIndex = indexImage, SelectedImageIndex = indexImage };
                    treeNodeCollection.Add(xnode);

                    tags = new Dictionary<string, object>();

                    ReadParameters(xelement, xnode);

                    xnode.Tag = tags;
                    LoadXmlToTreeView(xelement, xnode.Nodes);
                }
            }
        }
        #endregion LoadXmlToTreeView

        #region ReadParameters
        private void ReadParameters(XElement xelement, TreeNode xnode)
        {
            // Преобразовать в List и потом обратно в IEnumerable<XAttribute> после смены имен на локализованные ????
            IEnumerable<XAttribute> ieatr = xelement.Attributes();
            if (ieatr.Count() > 0)
            {
                List<XAttribute> lattr = ieatr.ToList();
                List<XAttribute> lattrNew = new List<XAttribute>();

                foreach (var atr in lattr) // in ieatr
                {
                    // Изменение имен Аттрибутов для интерфейса GetAttributeName(atr);
                    XAttribute xattr = new XAttribute(GetAttributeName(atr), atr.Value);

                    xnode.Nodes.Add(new TreeNode { Text = GetAttributeName(atr), Tag = xattr, ImageIndex = 1, SelectedImageIndex = 1 }); // TEST

                    lattrNew.Add(xattr);
                }

                IEnumerable<XAttribute> ieAttrNew = lattrNew.AsEnumerable();

                if (!tags.ContainsKey("Attributes")) // Тут Аттрибуты остаются английские
                {
                    tags.Add("Attributes", ieAttrNew); // "Attributes", xelement.Attributes()
                }
            }

            IEnumerable<XText> textsNode = xelement.Nodes().OfType<XText>().Where(t => t.NodeType != XmlNodeType.CDATA); // Выбирает все текстовые поля исключая CDATA
            if (textsNode.Any())
            {
                if (!tags.ContainsKey("Text"))
                {
                    tags.Add("Text", textsNode);
                }
                foreach (var text in textsNode)
                {
                    xnode.Nodes.Add(new TreeNode { Text = "#text", Tag = text.Value, ImageIndex = 23, SelectedImageIndex = 23 });
                }
            }

            IEnumerable<XComment> commentsNode = xelement.Nodes().OfType<XComment>();
            if (commentsNode.Any()) // 
            {
                if (!tags.ContainsKey("Comment"))
                {
                    tags.Add("Comment", commentsNode);
                }
                foreach (var comment in commentsNode)
                {
                    xnode.Nodes.Add(new TreeNode { Text = "#comment", Tag = comment, ImageIndex = 24, SelectedImageIndex = 24 });
                }
            }

            IEnumerable<XCData> cdataNode = xelement.Nodes().OfType<XCData>();
            if (cdataNode.Any()) // 
            {
                if (!tags.ContainsKey("CData"))
                {
                    tags.Add("CData", cdataNode);
                }
                foreach (var cdata in cdataNode)
                {
                    xnode.Nodes.Add(new TreeNode { Text = "#cdata-section", Tag = cdata });
                }
            }
        }
        #endregion ReadParameters

        #region GetAttributeName Интерфейсная часть
        // Изменение имен атрибутов для языкового вариианта интерфейса
        private string GetAttributeName(XAttribute attribute)
        {
            string aName = attribute.Name.ToString();

            // Поиск по имени xml Ноды имени параметра и подстановка индексов  изображений из словаря для отображения картинок.
            aName = CurLang[attribute.Name.ToString()].Replace(" ", "");

            if (!xml_node.ContainsKey(aName))
            {
                xml_node.Add(aName, attribute.Name.ToString());
            }
            return aName;
        }
        #endregion GetAttributeName


        // Создать какой-то список или словарь int для индексов Картинок и сжать код до проверки.
        #region ReadXName
        private void ReadXName(XElement xelement, out int indexImage, out XName xname)
        {
            indexImage = 0;
            xname = xelement.Name;

            string CurName = CurLang[xelement.Name.ToString()].Replace(" ", "");

            string nameXmlNode = xelement.Name.ToString();

            if (imageIndex.ContainsKey(nameXmlNode))
            {
                int idxImage = imageIndex[nameXmlNode];
                indexImage = idxImage;
                xname = CurName;
                if (!xml_node.ContainsKey(xname.ToString()))        // Заносим в словарь в качестве ключа языковое название ноды, в качестве значения английский вариант
                    xml_node.Add(CurName, nameXmlNode);
            }

            // Интерфейс - Иконки Нод TreeView и Названия Нод - Остальные были через else if
            //if (xelement.Name == "Chickens") // "Chickens"
            //{
            //    indexImage = 5;                                     // Указать изображение согласно типу птицы или животного из ImageList
            //    xname = xelement.Name = ModulePhrases.ChickensNode; // Интерфейс
            //    if (!xml_node.ContainsKey(xname.ToString()))        // Заносим в словарь в качестве ключа языковое название ноды, в качестве значения английский вариант
            //        xml_node.Add(xname.ToString(), "Chickens");
            //}
            // Интерфейс - Иконки Нод TreeView и Названия Нод - Остальные были через else if
        }
        #endregion ReadXName

        #region SaveXmlItems  - Рабочий вариант, который допилился, возможно можно допилить и вариант выше
        // Еше вариант
        public void SaveItems(XElement curNode, TreeNode item)
        {
            XAttribute xAttribute;
            XText xText;
            XComment xComment;
            XCData xCData;

            foreach (TreeNode itemloc in item.Nodes)
            {
                XElement newNode;

                if (itemloc.Text == "#text")
                {
                    xText = new XText(itemloc.Tag.ToString());
                    curNode.Add(xText);
                }
                else if (itemloc.Text == "#comment")
                {
                    xComment = new XComment(((XComment)itemloc.Tag).Value);
                    curNode.Add(xComment);
                }
                else if (itemloc.Text == "#cdata-section")
                {
                    xCData = new XCData((XCData)itemloc.Tag);
                    curNode.Add(xCData);
                }
                else if (itemloc.Nodes.Count == 0) // Проверка на отсутствующие ноды у дочерних, либо это атрибут, либо пустой элемент
                {
                    if (itemloc.Tag is XAttribute) // проверка на атрибут
                    {
                        if (xml_node.ContainsKey(itemloc.Text))
                            xAttribute = new XAttribute(xml_node[itemloc.Text], ((XAttribute)itemloc.Tag).Value);
                        else
                            xAttribute = new XAttribute(itemloc.Text, ((XAttribute)itemloc.Tag).Value);
                        curNode.Add(xAttribute);
                    }
                    else
                    {
                        if (xml_node.ContainsKey(itemloc.Text))
                            curNode.Add(new XElement(xml_node[itemloc.Text]));
                        else
                            curNode.Add(new XElement(itemloc.Text));
                    }
                }
                else
                {
                    if (xml_node.ContainsKey(itemloc.Text))
                        newNode = new XElement(xml_node[itemloc.Text]);
                    else
                        newNode = new XElement(itemloc.Text);
                    SaveItems(newNode, itemloc);
                    curNode.Add(newNode);
                }
            }
        }
        // Еше вариант
        #endregion SaveXmlItems

        #region Modify_when_AfterSelect
        private void trvRoot_AfterSelect(object sener, TreeViewEventArgs e)
        {
            trvRoot.SelectedNode = e.Node;
            if (trvRoot.SelectedNode.Tag == null) return;

            TreeNode selectNode = trvRoot.SelectedNode;
            LoadWindow(selectNode);
        }
        #endregion Modify_when_AfterSelect

        private void LoadWindow(TreeNode selectNode)
        {
            if (trvRoot.SelectedNode == null)
            {
                return;
            }

            // checking tabs
            ValidateTabPage();

            frmParameters = new FrmParameters(tStrip); // Передаем весь ToolStrip чтобы вынимать из него нужные кнопки для подписки
            frmParameters.frmParentGloabal = this;

            try
            {
                TabPage tabPageNew = new TabPage(trvRoot.SelectedNode.Text);
                tabPageNew.Name = trvRoot.SelectedNode.Text;
                frmParameters.Name = trvRoot.SelectedNode.Text;

                tabPageNew.Text = trvRoot.SelectedNode.Text;
                tabPageNew.ImageKey = trvRoot.SelectedNode.ImageKey;
                tabPageNew.Controls.Add(frmParameters); // Добавление Формы в Контрол TabPage

                tabControl.ImageList = imgList; // Добавил картинки - что дальше ?
                tabControl.TabPages.Add(tabPageNew);
                tabControl.SelectedTab = tabPageNew;

                frmParameters.Dock = DockStyle.Fill;
                frmParameters.Show();
            }
            catch (ObjectDisposedException) { }
        }

        #region ValidateTabPage
        private void ValidateTabPage()
        {
            if (frmParameters != null)
            {
                frmParameters.DialogResult = DialogResult.OK;
                frmParameters.Close();
                frmParameters.Dispose();
            }
            tabControl.TabPages.Clear();
        }
        #endregion ValidateTabPage

        #region ButtonOpenClick
        private void btOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = ModulePhrases.FilterProject; // "Data files |*.xml|All iles|*.*"

            OFD.FileName = "";
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                filePath = Path.GetFileName(OFD.FileName);
            }

            trvRoot.Nodes.Clear();
            xml_node.Clear();

            trvRoot.ImageList = imgList;

            LoadTemplate(OFD.FileName);
        }
        #endregion ButtonOpenClick

        #region CreateMenu
        private void CreateMenu()
        {
            // Create the ContextMenuStrip.
            contextMenu = new ContextMenuStrip();

            //Create some menu items.
            submenuGroup.Text = ModulePhrases.AddGroup; // "Add Group"
            submenuGroup.Image = imgList.Images[0];

            ToolStripMenuItem addDevice = new ToolStripMenuItem();
            addDevice.Text = ModulePhrases.AddDevice; // "Add Device"
            addDevice.Image = imgList.Images[2];

            ToolStripMenuItem addParameters = new ToolStripMenuItem();
            addParameters.Text = ModulePhrases.AddParameters; // "Add Parameters"
            addParameters.Image = imgList.Images[1];

            ToolStripMenuItem addСalendar = new ToolStripMenuItem();
            addСalendar.Text = ModulePhrases.AddCalendar; // "Add Daily Calendar"
            addСalendar.Image = imgList.Images[20];

            ToolStripMenuItem addDay = new ToolStripMenuItem();
            addDay.Text = ModulePhrases.AddDay; // "Add Day"
            addDay.Image = imgList.Images[21];

            ToolStripMenuItem addPrograms = new ToolStripMenuItem();
            addPrograms.Text = ModulePhrases.AddPrograms; // "Add Programs"
            addPrograms.Image = imgList.Images[22];

            ToolStripMenuItem addProgram = new ToolStripMenuItem();
            addProgram.Text = ModulePhrases.AddProgram; // "Add Program"
            addProgram.Image = imgList.Images[19];

            ToolStripMenuItem addInputs = new ToolStripMenuItem();
            addInputs.Text = ModulePhrases.AddInputs; // "Add Inputs"
            addInputs.Image = imgList.Images[17];

            ToolStripMenuItem addOutputs = new ToolStripMenuItem();
            addOutputs.Text = ModulePhrases.AddOutputs; // "Add Outputs"
            addOutputs.Image = imgList.Images[18];

            ToolStripMenuItem addInput = new ToolStripMenuItem();
            addInput.Text = ModulePhrases.AddInput; // "Add Input"
            addInput.Image = imgList.Images[17];

            ToolStripMenuItem addOutput = new ToolStripMenuItem();
            addOutput.Text = ModulePhrases.AddOutput; // "Add Output"
            addOutput.Image = imgList.Images[18];

            ToolStripMenuItem addComment = new ToolStripMenuItem();
            addComment.Text = ModulePhrases.AddComment;
            addComment.Image = imgList.Images[24];

            ToolStripMenuItem addText = new ToolStripMenuItem();
            addText.Text = ModulePhrases.AddText;
            addText.Image = imgList.Images[23];

            ToolStripMenuItem addDelete = new ToolStripMenuItem();
            addDelete.Text = ModulePhrases.Delete;
            addDelete.Image = imgList.Images[25];

            //Add the menu items to the menu.
            contextMenu.Items.AddRange(new ToolStripMenuItem[]{submenuGroup, addDevice, addParameters, addСalendar, addDay, addPrograms, addProgram,
                addInputs, addOutputs, addInput, addOutput, addComment, addText, addDelete });

            contextMenu.ItemClicked += ContextMenu_ItemClicked;
            submenuGroup.DropDownItemClicked += SubmenuGroup_DropDownItemClicked;
        }

        #region CreateParameters
        // Метод добавления параметров для использования сразу при добавлении соответствующей ноды
        private void CreateParameters(string nodeText)
        {
            if (nodeText == ModulePhrases.Device) // Добавление параметров для Устройств (Device)
            {
                Farm.Device device = new Farm.Device
                { DeviceName = "", Location = "", DeviceType = "", Active = false };

                XmlSerializer serializer = new XmlSerializer(typeof(Farm.Device));
                AddParameters(serializer, device);
            }
            else if (nodeText == ModulePhrases.Program)
            {
                Farm.Programs.ProgramX program = new Farm.Programs.ProgramX
                { Task = "", TaskPriority = "", TaskCycle = "", ProgramType = "", ProgramActive = false };

                XmlSerializer serializer = new XmlSerializer(typeof(Farm.Programs.ProgramX));
                AddParameters(serializer, program);
            }
            else if (nodeText == ModulePhrases.Day) // trvRoot.SelectedNode.Text TEST - Как сделать добавление единичных параметров из списка Cooling - или добавить чтобы можно было добавлять его отдельно
            {
                Farm.Calendar.DayX day = new Farm.Calendar.DayX
                {
                    TempA = "",
                    iTempA = "",
                    TempS = "",
                    iTempS = "",
                    Humidity = "",
                    iHumidity = "",
                    Cooling = true,
                    CoolAmount = "1",
                    CoolTemp = "20",
                    CoolTime = "5",
                    CooliTemp = "1" // Количество 1, Температруа 20, Время 5 минут, Нечувствительность 1 гр.
                };

                XmlSerializer serializer = new XmlSerializer(typeof(Farm.Calendar.DayX));
                AddParameters(serializer, day);
            }
            else if (nodeText == ModulePhrases.Input)
            {
                Farm.Programs.ProgramX.Input input = new Farm.Programs.ProgramX.Input
                {
                    ValName = "",
                    CnlCode = "",
                    CnlNum = "",
                    Initialize = false,
                    Value = ""
                };

                XmlSerializer serializer = new XmlSerializer(typeof(Farm.Programs.ProgramX.Input));
                AddParameters(serializer, input);
            }
            else if (nodeText == ModulePhrases.Output)
            {
                Farm.Programs.ProgramX.Output output = new Farm.Programs.ProgramX.Output
                {
                    ValName = "",
                    CnlCode = "",
                    CnlNum = "",
                    Retain = false,
                    Value = ""
                };

                XmlSerializer serializer = new XmlSerializer(typeof(Farm.Programs.ProgramX.Output));
                AddParameters(serializer, output);
            }
            else if (nodeText == ModulePhrases.MainNode)
            {
                Farm farm = new Farm
                {
                    VisibleLogPrgFile = true,
                    SaveLogPrgTime = "2",
                    IterationTime = "300",
                };
                XmlSerializer serializer = new XmlSerializer(typeof(Farm));
                AddParameters(serializer, farm);
            }
        }
        #endregion CreateParameters

        #region AddParameters_Void
        private void AddParameters(XmlSerializer serializer, object obj)
        {
            XDocument xdoc = new XDocument();
            //Create our own namespaces for the output
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            //Add an empty namespace and empty value
            ns.Add("", "");

            using (MemoryStream w = new MemoryStream())
            {
                serializer.Serialize(w, obj, ns);
                w.Position = 0;
                xdoc = XDocument.Load(w);

                XElement rootElement = xdoc.Root;
                // Сперва читаем словарь объектов текущей ноды
                tags = trvRoot.SelectedNode.Tag as Dictionary<string, object>;

                ReadParameters(rootElement, trvRoot.SelectedNode);
                // Тут добавить в родительскую Ноду Список аттрибутов со значениями 
                trvRoot.SelectedNode.Tag = tags;
                w.Close();
            }
            trvRoot.SelectedNode.Expand(); // Раскрыть текущую ноду после добавления
            modified = true;
        }
        #endregion AddParameters_Void


        private void ContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // Сперва читаем словарь объектов текущей ноды
            tags = trvRoot.SelectedNode.Tag as Dictionary<string, object>;

            #region Delete_Attribute
            if (e.ClickedItem.Text == ModulePhrases.Delete) // "Delete"
            {
                Dictionary<string, object> TagDict = trvRoot.SelectedNode.Parent.Tag as Dictionary<string, object>;
                string nodeText = trvRoot.SelectedNode.Text;

                // Проверка удаляемой ноды на Атрибут
                bool isAttr = trvRoot.SelectedNode.Tag is XAttribute;
                if (isAttr)
                {
                    if (TagDict.ContainsKey("Attributes"))
                    {
                        List<XAttribute> lattr = TagDict["Attributes"] as List<XAttribute>;
                        XAttribute xattr = trvRoot.SelectedNode.Tag as XAttribute;

                        // Индекс удаляемого атрибута
                        XAttribute findXattr = lattr.Where(x => x.Name == xattr.Name).FirstOrDefault();
                        int attrIndex = lattr.IndexOf(findXattr); // xattr
                        lattr.RemoveAt(attrIndex);

                        // Записать все обратно в Tag родителя
                        TagDict["Attributes"] = lattr;
                        if (lattr.Count == 0)
                        {
                            TagDict.Remove("Attributes"); // Удалить ключ словаря
                        }
                        trvRoot.SelectedNode.Parent.Tag = TagDict;
                        // Записать все обратно в Tag родителя
                    }
                }

                // Удаление текстовой метки и словаря
                if (trvRoot.SelectedNode.Text == "#text")
                {
                    TagDict.Remove("Text"); // Test
                    trvRoot.SelectedNode.Parent.Tag = TagDict;
                }

                trvRoot.SelectedNode.Remove();
                modified = true;
            }
            #endregion Delete_Attribute

            #region AddParameters
            else if (e.ClickedItem.Text == ModulePhrases.AddParameters) // Добавить в словарь "Add Parameters"
            {
                //XDocument xdoc = new XDocument();
                CreateParameters(trvRoot.SelectedNode.Text);
            }
            #endregion AddParameters

            #region AddComment
            else if (e.ClickedItem.Text == ModulePhrases.AddComment) // "Add Comment"
            {
                if (tags == null)
                {
                    tags = new Dictionary<string, object>();
                }

                XComment comment = new XComment("");
                XElement xelement = new XElement("Comment");
                xelement.Add(comment);

                ReadParameters(xelement, trvRoot.SelectedNode);
                // Тут добавить в родительскую Ноду новые данные 
                trvRoot.SelectedNode.Tag = tags;
                trvRoot.SelectedNode.Expand(); // Раскрыть текущую ноду после добавления
            }
            #endregion AddComment

            #region AddText
            else if (e.ClickedItem.Text == ModulePhrases.AddText) // "Add Text"
            {
                if (tags == null)
                {
                    tags = new Dictionary<string, object>();
                }

                XText xtext = new XText("");
                XElement xelement = new XElement("Text");
                xelement.Add(xtext);

                ReadParameters(xelement, trvRoot.SelectedNode);
                // Тут добавить в родительскую Ноду новые данные 
                trvRoot.SelectedNode.Tag = tags;
                trvRoot.SelectedNode.Expand(); // Раскрыть текущую ноду после добавления
            }
            #endregion AddText

            #region AddNode
            // Собрать все варианты добавления просто Нод в одну кучу, определив имя элемента через словари соответствий
            else if (e.ClickedItem.Text == ModulePhrases.AddDevice || e.ClickedItem.Text == ModulePhrases.AddCalendar
                || e.ClickedItem.Text == ModulePhrases.AddDay || e.ClickedItem.Text == ModulePhrases.AddPrograms
                || e.ClickedItem.Text == ModulePhrases.AddProgram || e.ClickedItem.Text == ModulePhrases.AddInputs
                || e.ClickedItem.Text == ModulePhrases.AddOutputs || e.ClickedItem.Text == ModulePhrases.AddInput
                || e.ClickedItem.Text == ModulePhrases.AddOutput)
            {

                trvRoot.BeginUpdate();
                int indexImage = 0;
                XName xname = "AnyName";
                XElement xelement = new XElement(xname);

                var nameKey = CurLang.Where(x => x.Value == e.ClickedItem.Text).FirstOrDefault().Key;

                xelement = new XElement(PrevLang[nameKey].Replace("Add ", ""));
                ReadXName(xelement, out indexImage, out xname);

                TreeNode xnode = new TreeNode { Text = xname.ToString(), ImageIndex = indexImage, SelectedImageIndex = indexImage };
                trvRoot.SelectedNode.Nodes.Add(xnode);
                trvRoot.SelectedNode.Expand(); // Раскрыть текущую ноду после добавления
                trvRoot.SelectedNode = trvRoot.SelectedNode.LastNode;
                trvRoot.EndUpdate();

                tags = new Dictionary<string, object>();
                ReadParameters(xelement, trvRoot.SelectedNode);
                trvRoot.SelectedNode.Tag = tags;

                CreateParameters(trvRoot.SelectedNode.Text); // TEST Надо после Add перейти и выбрать добавленную ноду
                LoadWindow(trvRoot.SelectedNode); // TEST
                // Как сделать обновление ноды ?, типа выбора
            }
            #endregion AddNode
        }
        #endregion CreateMenu

        // TEST - Можно ли упростить код создания SubMenu ?
        #region SubMenu
        private void SubMenuItem()
        {
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = ModulePhrases.Chickens;
            item.Image = imgList.Images[5];
            submenuGroup.DropDownItems.Add(item);

            item = new ToolStripMenuItem();
            item.Text = ModulePhrases.Gooses;
            item.Image = imgList.Images[12];
            submenuGroup.DropDownItems.Add(item);

            item = new ToolStripMenuItem();
            item.Text = ModulePhrases.Turkeys;
            item.Image = imgList.Images[9];
            submenuGroup.DropDownItems.Add(item);

            item = new ToolStripMenuItem();
            item.Text = ModulePhrases.Quails;
            item.Image = imgList.Images[13];
            submenuGroup.DropDownItems.Add(item);

            item = new ToolStripMenuItem();
            item.Text = ModulePhrases.Ducks;
            item.Image = imgList.Images[7];
            submenuGroup.DropDownItems.Add(item);

            item = new ToolStripMenuItem();
            item.Text = ModulePhrases.Ostriches;
            item.Image = imgList.Images[15];
            submenuGroup.DropDownItems.Add(item);

            item = new ToolStripMenuItem();
            item.Text = ModulePhrases.Cows;
            item.Image = imgList.Images[8];
            submenuGroup.DropDownItems.Add(item);

            item = new ToolStripMenuItem();
            item.Text = ModulePhrases.Sheeps;
            item.Image = imgList.Images[10];
            submenuGroup.DropDownItems.Add(item);

            item = new ToolStripMenuItem();
            item.Text = ModulePhrases.Goats;
            item.Image = imgList.Images[14];
            submenuGroup.DropDownItems.Add(item);

            item = new ToolStripMenuItem();
            item.Text = ModulePhrases.Pigs;
            item.Image = imgList.Images[6];
            submenuGroup.DropDownItems.Add(item);

            item = new ToolStripMenuItem();
            item.Text = ModulePhrases.Rabbits;
            item.Image = imgList.Images[11];
            submenuGroup.DropDownItems.Add(item);

            item = new ToolStripMenuItem();
            item.Text = ModulePhrases.Others;
            item.Image = imgList.Images[16];
            submenuGroup.DropDownItems.Add(item);
        }
        #endregion SubMenu

        #region DropDownItemClicked
        private void SubmenuGroup_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int indexImage = 0;
            XName xname = "Others";

            var nameKey = CurLang.Where(x => x.Value == e.ClickedItem.Text).FirstOrDefault().Key;
            xname = PrevLang[nameKey];

            XElement element = new XElement(xname);
            ReadXName(element, out indexImage, out xname);

            TreeNode xnode = new TreeNode { Text = xname.ToString(), ImageIndex = indexImage, SelectedImageIndex = indexImage };
            trvRoot.SelectedNode.Nodes.Add(xnode);
            trvRoot.SelectedNode.Expand(); // Раскрыть текущую ноду после добавления
        }
        #endregion DropDownItemClicked

        #region CreateEmptyRootNode
        private void CreateRootNode()
        {
            XDocument document = new XDocument();
            XElement rootElement = new XElement(ModulePhrases.MainNode);
            TreeNode rootNode = new TreeNode { Text = rootElement.Name.ToString(), ImageIndex = 0, SelectedImageIndex = 0 };

            trvRoot.Nodes.Add(rootNode); // Наш TreeView

            // TEST
            if (xml_node.Count > 0)
            {
                foreach (var ff in xml_node)
                {
                    rtb_Log.Text += $"Key {ff.Key}  Value {ff.Value}" + Environment.NewLine;
                }
            }
            else
            {
                rtb_Log.Text += $"xml_node.Count {xml_node.Count}" + Environment.NewLine;
            }
            // TEST
        }
        #endregion CreateEmptyRootNode

        #region LoadTemplate
        private void LoadTemplate(string filepath)
        {
            if (filepath != null)
            {
                var start = DateTime.Now; // Запуск измерения времени

                //Use xDocument to read xml file
                XDocument document = XDocument.Load(filepath);
                //Remove the root node
                XElement rootElement = document.Root;

                XName name = rootElement.Name = ModulePhrases.MainNode; // Интерфейс

                if (!xml_node.ContainsKey(name.ToString()))
                    xml_node.Add(name.ToString(), "Farm");

                //Load the root element of the xml file to the root node of the treeview
                TreeNode rootNode = new TreeNode { Text = name.ToString(), ImageIndex = 0, SelectedImageIndex = 0 }; // Index 0 = Device image

                trvRoot.Nodes.Add(rootNode); // Наш TreeView

                tags = new Dictionary<string, object>();

                ReadParameters(rootElement, rootNode);

                rootNode.Tag = tags;

                //Load XML into TreeView with recursion
                LoadXmlToTreeView(rootElement, rootNode.Nodes);

                #region EmptyRoot если не требуется атрибутов в root директории можно упростить
                //// Когда в рут директории не нужны атрибуты, можно упростить до такого кода
                //TreeNode rootNode = new TreeNode { Text = rootElement.Name.ToString(), ImageIndex = 0, SelectedImageIndex = 0 }; // Index 0 = Device image
                //trvRoot.Nodes.Add(rootNode); // TreeNode rootNode = trvRoot.Nodes.Add(rootElement.Name.ToString());
                //LoadxmlToTreeView(rootElement, rootNode.Nodes);
                #endregion EmptyRoot

                rootNode.Expand();

                // Измерение времени и вывод затраченного времени
                var elapsed = DateTime.Now.Subtract(start);
                lblFeedback.Text = string.Format("TreeViewLoad: {0:N0} ms ({1})", elapsed.TotalMilliseconds, elapsed.Display());
            }
        }
        #endregion LoadTemplate

        #region Form_Load
        private void FrmModuleConfig_Load(object sender, EventArgs e)
        {
            // translate the form
            FormTranslator.Translate(this, GetType().FullName);

            // translate TreeNode
            //FormTranslator.Translate(cmnuListMain, GetType().FullName);

            CurLang.Clear();
            CurLang = ModFarmView.GetLangTo();

            if (File.Exists(configFileName))
            {
                trvRoot.Nodes.Clear();
                xml_node.Clear();

                trvRoot.ImageList = imgList;
                LoadTemplate(configFileName);
            }
        }
        #endregion Form_Load

        #region NewButtonClick
        private void tbNew_Click(object sender, EventArgs e)
        {
            trvRoot.Nodes.Clear();
            xml_node.Clear();
            trvRoot.ImageList = imgList;
            CreateRootNode();
        }
        #endregion NewButtonClick

        private void btOK_Click(object sender, EventArgs e)
        {
            fileName = configFileName;
            tbSave_Click(sender, e);
            modified = false;
        }

        private void tbSaveAs_Click(object sender, EventArgs e)
        {
            SFD.InitialDirectory = appDirs.ConfigDir;
            SFD.Title = ModulePhrases.TitleLoadProject;
            SFD.Filter = ModulePhrases.FilterProject;

            if (shortFileName == "")
            {
                if (SFD.ShowDialog() == DialogResult.OK)
                {
                    fileName = SFD.FileName;
                }
                else return;
            }

            // сохранение шаблона устройства в новый файл
            tbSave_Click(sender, e);
        }

        #region SaveButtion
        private void tbSave_Click(object sender, EventArgs e)
        {
            var start = DateTime.Now; // TEST

            if (fileName == null) fileName = configFileName;

            XElement root;
            // Сохраняем файл
            if (xml_node.ContainsKey(trvRoot.Nodes[0].Text.ToString()))
                root = new XElement(xml_node[trvRoot.Nodes[0].Text.ToString()]);
            else
                root = new XElement(trvRoot.Nodes[0].Text.ToString());

            foreach (TreeNode item in trvRoot.Nodes)
                SaveItems(root, item);
            root.Save(fileName); //  root.Save(configFileName);

            modified = false;
            btOK.Enabled = false;
            // Измерение времени и вывод затраченного времени
            var elapsed = DateTime.Now.Subtract(start); // TEST
            lblFeedback.Text = string.Format("TreeViewSave: {0:N0} ms ({1})", elapsed.TotalMilliseconds, elapsed.Display()); // TEST
        }
        #endregion SaveButtion


        // Выбор ноды при правой кнопке мыши
        #region MouseRight
        private void trvRoot_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Если нажата правая кнопка мыши, проверяем наличие уже добавленных элементов для отключения возможности добавить.
            if (e.Button == MouseButtons.Right)
            {
                trvRoot.SelectedNode = e.Node;
                tags = e.Node.Tag as Dictionary<string, object>;
                List<TreeNode> treeNodeList = trvRoot.SelectedNode.Nodes.Cast<TreeNode>().ToList();

                // Проверить добавленные Ноды птиц, животных и закрыть к добавлению тех, что есть.
                if (e.Node.Text == ModulePhrases.MainNode)
                {
                    List<ToolStripMenuItem> listTSMI = submenuGroup.DropDownItems.Cast<ToolStripMenuItem>().ToList();
                    foreach (ToolStripMenuItem item in listTSMI)
                    {
                        TreeNode tnl = treeNodeList.Where(n => n.Text == item.Text).FirstOrDefault();
                        if (tnl != null) item.Enabled = false;
                        else item.Enabled = true;
                    }
                }
                else if (e.Node.Text == ModulePhrases.Device)
                {
                    // Проверить наличие Календаря и Программ
                    TreeNode tnl = treeNodeList.Where(x => x.Text == ModulePhrases.Calendar).FirstOrDefault();
                    if (tnl != null) contextMenu.Items[3].Enabled = false;
                    else contextMenu.Items[3].Enabled = true;

                    tnl = treeNodeList.Where(x => x.Text == ModulePhrases.Programs).FirstOrDefault();
                    if (tnl != null) contextMenu.Items[5].Enabled = false;
                    else contextMenu.Items[5].Enabled = true;
                }
                else if (e.Node.Text == ModulePhrases.Program)
                {
                    // Проверить наличие Входов, Выходов в Программе
                    TreeNode tnl = treeNodeList.Where(x => x.Text == ModulePhrases.Inputs).FirstOrDefault();
                    if (tnl != null) contextMenu.Items[7].Enabled = false;
                    else contextMenu.Items[7].Enabled = true;

                    tnl = treeNodeList.Where(x => x.Text == ModulePhrases.Outputs).FirstOrDefault();
                    if (tnl != null) contextMenu.Items[8].Enabled = false;
                    else contextMenu.Items[8].Enabled = true;
                }

                if (tags != null && tags.ContainsKey("Attributes"))
                {
                    // Закрыть параметр AddParameters
                    contextMenu.Items[2].Enabled = false;
                }
                else contextMenu.Items[2].Enabled = true; // Открыть меню добавления Параметров

                //if (tags != null && tags.ContainsKey("Text"))
                //{
                //    // Закрыть параметр AddText
                //    contextMenu.Items[12].Enabled = false;
                //}
                //else contextMenu.Items[12].Enabled = true; // Открыть меню добавления Текста
                // TEST почему в атрибутах закрытие Текста, а индекс Параметров?
            }
        }
        #endregion MouseRight


        // Отображение контекстного меню в зависимости от Ноды
        #region MouseDown
        private void trvRoot_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                TreeViewHitTestInfo htInfo = trvRoot.HitTest(e.X, e.Y);
                TreeNode selectNode = htInfo.Node;
                if (selectNode != null)
                {
                    trvRoot.SelectedNode = selectNode;
                    trvRoot.SelectedNode.ContextMenuStrip = null; // TEST
                    string nodeText = trvRoot.SelectedNode.Text;

                    // [0] submenuGroup, [1] addDevice, [2] addParameter, [3] addDailyСalendar, [4] addDay, [5] addPrograms,  [6] addProgram,
                    // [7] Inputs, [8] Outputs, [9] Input, [10] Output, [11] addComment, [12] addText, [13] addDelete

                    contextMenu.Items[0].Available = false;     // submenuGroup
                    contextMenu.Items[1].Available = false;     // Device
                    contextMenu.Items[2].Available = false;     // Parameter
                    contextMenu.Items[3].Available = false;     // Calendar
                    contextMenu.Items[4].Available = false;     // Day
                    contextMenu.Items[5].Available = false;     // Programs
                    contextMenu.Items[6].Available = false;     // Program
                    contextMenu.Items[7].Available = false;     // Inputs
                    contextMenu.Items[8].Available = false;     // Outputs
                    contextMenu.Items[9].Available = false;     // Input 
                    contextMenu.Items[10].Available = false;    // Output
                    contextMenu.Items[11].Available = true;     // Comment
                    contextMenu.Items[12].Available = false;    // Text
                    contextMenu.Items[13].Available = true;     // Delete

                    if (nodeText == ModulePhrases.MainNode)
                    {
                        contextMenu.Items[0].Available = true;     // submenuGroup
                        contextMenu.Items[2].Available = true;
                        contextMenu.Items[13].Available = false;   // Delete
                    }
                    else if (nodeText == ModulePhrases.Device)
                    {
                        contextMenu.Items[2].Available = true;
                        contextMenu.Items[3].Available = true;
                        contextMenu.Items[5].Available = true;
                    }
                    else if (nodeText == ModulePhrases.Calendar)
                    {
                        contextMenu.Items[4].Available = true;
                    }
                    else if (nodeText == ModulePhrases.Day)
                    {
                        contextMenu.Items[2].Available = true;     // Parameter
                    }
                    else if (nodeText == ModulePhrases.Programs)
                    {
                        contextMenu.Items[6].Available = true;
                    }
                    else if (nodeText == ModulePhrases.Program)
                    {
                        contextMenu.Items[2].Available = true;
                        contextMenu.Items[7].Available = true;
                        contextMenu.Items[8].Available = true;
                    }
                    else if (nodeText == ModulePhrases.Inputs)
                    {
                        contextMenu.Items[9].Available = true;
                    }
                    else if (nodeText == ModulePhrases.Outputs)
                    {
                        contextMenu.Items[10].Available = true;
                    }
                    else if (nodeText == ModulePhrases.Input || nodeText == ModulePhrases.Output)
                    {
                        contextMenu.Items[2].Available = true;
                    }
                    else if (nodeText == "#comment" || nodeText == "#text" || nodeText == "#cdata-section" || selectNode.Tag is XAttribute)
                    {
                        contextMenu.Items[11].Available = false;    // Comment
                    }
                    else
                    {
                        // Сюда входят группы птиц и животных
                        contextMenu.Items[1].Available = true;
                    }

                    trvRoot.SelectedNode.ContextMenuStrip = contextMenu; // TEST
                }
            }
            catch { }
        }
        #endregion MouseDown

        #region Button_Cancel
        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion Button_Cancel

        #region Form_Closing
        private void FrmModuleConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (modified)
            {
                DialogResult result = MessageBox.Show(ServerPhrases.SaveModuleConfigConfirm,
                    CommonPhrases.QuestionCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Yes:
                        fileName = configFileName;
                        tbSave_Click(sender, e);
                        break;

                    case DialogResult.No:
                        break;

                    default:
                        e.Cancel = true;
                        break;
                }
            }
        }
        #endregion Form_Closing


        #region TraceObject
        void TraceObjectFields(object obj) // выводит список полей, их имена и значения:
        {
            foreach (FieldInfo mi in obj.GetType().GetFields())
            {
                rtb_Log.Text += $"{mi.FieldType.Name} {mi.Name} = {mi.GetValue(obj)} {mi.FieldType.ToString()}" + Environment.NewLine;
            }
            //moduleLog.WriteLine(mi.FieldType.Name + " " + mi.Name + " = " + mi.GetValue(obj) + mi.FieldType.ToString());
        }
        void TraceObjectProperties(object obj) // выводит список свойств объекта:
        {
            foreach (PropertyInfo pi in obj.GetType().GetProperties())
            {
                rtb_Log.Text += $"{pi.PropertyType.Name} {pi.Name} = {pi.GetValue(obj, null)}" + Environment.NewLine;
            }
            //moduleLog.WriteLine(pi.PropertyType.Name + " " + pi.Name + " = " + pi.GetValue(obj, null));
        }
        void TraceObjectMethods(object obj)
        {
            foreach (MethodInfo mi in obj.GetType().GetMethods())
            {
                rtb_Log.Text += $"{mi.Name} {mi.IsPublic}" + Environment.NewLine;
            }
            //moduleLog.WriteLine(mi.Name + " " + mi.IsPublic);
        }
        #endregion TraceObject
    }

    #region TimeSpanExpander - В последствии не нужен, чисто для измерения времени чтения/записи
    public static class TimeSpanExtender
    {
        /// <summary>
        /// Present timespan in friendly form adequate for the size.
        /// </summary>
        public static string Display(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
                return string.Format("{0:N1} days", timeSpan.TotalDays);
            if (timeSpan.TotalHours >= 1)
                return string.Format("{0:N1} h", timeSpan.TotalHours);
            if (timeSpan.TotalMinutes >= 1)
                return string.Format("{0:N1} min", timeSpan.TotalMinutes);
            if (timeSpan.TotalSeconds >= 1)
                return string.Format("{0:N1} s", timeSpan.TotalSeconds);
            if (timeSpan.TotalMilliseconds >= 10)
                return string.Format("{0:N0} ms", timeSpan.TotalMilliseconds);
            if (timeSpan.TotalMilliseconds >= 1)
                return string.Format("{0:N1} ms", timeSpan.TotalMilliseconds);
            double totalMicroseconds = timeSpan.TotalMilliseconds * 1000;
            if (totalMicroseconds >= 10)
                return string.Format("{0:N0} μs", totalMicroseconds);
            return string.Format("{0:N1} μs", totalMicroseconds);
        }
    }
    #endregion TimeSpanExpander
}
