form = WinForms.Form{
    Text="Ledger Sheet", Width=700, Height=500, StartPosition="CenterScreen",
    WinForms.SplitContainer {
        Dock="Fill", Width=700, SplitterDistance=200,
        WinForms.TreeView{ Name="treeviewCategory", Dock="Fill", HideSelection=false },
        WinForms.Panel{
            Dock="Fill",
            WinForms.ListView{ 
                Name="listviewEntries", Dock="Fill", View="Details", GridLines=true, FullRowSelect=true,
                ContextMenuStrip=WinForms.ContextMenuStrip {
                    WinForms.ToolStripMenuItem { Text="Delete", Click=function(sender,e) DeleteEntry() end }
                }
            },
            WinForms.StatusStrip { Name="statusStrip", Dock="Bottom" }
        }
    },
    WinForms.ToolStrip{
        Dock="Top", Top=0, Left=0, Width=700, Height=25,
        WinForms.ToolStripButton { Name="btnOpen", Text="&Open", Width=88, Height=22, Image="icon\\open.png" },
        WinForms.ToolStripButton { Name="btnSave", Text="&Save", Width=88, Height=22, Image="icon\\save.png" },
        WinForms.ToolStripButton { Name="btnAdd", Text="&Add", Width=88, Height=22, Image="icon\\add.png" },
    }
}

incomeNode = treeviewCategory.Nodes.Add("Income")
outgoNode = treeviewCategory.Nodes.Add("Outgo")
listviewEntries.Columns.Add(WinForms.ColumnHeader{ Text="Date", Width=100 })
listviewEntries.Columns.Add(WinForms.ColumnHeader{ Text="Detail", Width=260 })
listviewEntries.Columns.Add(WinForms.ColumnHeader{ Text="Amount", Width=120 })

dialog = WinForms.Form{
    Text="Add Entry", Width=320, Height=220, StartPosition="CenterParent", FormBorderStyle="FixedDialog", ShowInTaskbar = false;
    WinForms.Label{ Text="Subject:", Width=60, Height=17, Top=14, Left=12 },
    WinForms.RadioButton { Name="dialog_Income", Text="Income", Width=80, Height=20, Top=9, Left=80 },
    WinForms.RadioButton { Name="dialog_Outgo", Text="Outgo", Width=80, Height=20, Top=9, Left=160, Checked=true },
    WinForms.Label{ Text="Category:", Width=60, Height=17, Top=40, Left=12 },
    WinForms.ComboBox { Name="dialog_Category", Width=160, Height=20, Top=36, Left=80 },
    WinForms.Label{ Text="Detail:", Width=60, Height=17, Top=68, Left=12 },
    WinForms.TextBox { Name="dialog_Detail", Width=160, Height=20, Top=64, Left=80 },
    WinForms.Label{ Text="Amount:", Width=60, Height=17, Top=96, Left=12 },
    WinForms.TextBox { Name="dialog_Amount", Width=128, Height=20, Top=92, Left=80 },
    WinForms.Label{ Text="Date:", Width=60, Height=17, Top=128, Left=12 },
    WinForms.DateTimePicker { Name="dialog_Date", Width=128, Height=21, Top=124, Left=80, Format="Short" },
    WinForms.Button{ Text="OK", Name="dialog_btnOK", Width=80, Height=23, Top=156, Left=130, DialogResult="OK" },
    WinForms.Button{ Text="Cancel", Name="dialog_btnCancel", Width=80, Height=23, Top=156, Left=224, DialogResult="Cancel" },
    AcceptButton=dialog_btnOK,
    CancelButton=dialog_btnCancel
}

Entries = {}

btnAdd.Click = function (sender,e)
    dialog_Detail.Text = ""
    dialog_Amount.Text = ""
    if treeviewCategory.SelectedNode ~= nil and treeviewCategory.SelectedNode.Tag ~= nil then
        dialog_Category.Text = treeviewCategory.SelectedNode.Text
    end
    if dialog.ShowDialog(form) == "OK" then
        local subject = dialog_Income.Checked and "income" or "outgo"
        local category = dialog_Category.Text
        local detail = dialog_Detail.Text
        local amount = dialog_Amount.Text
        local date = dialog_Date.Value.ToShortDateString()
        local entry = {date, subject, category, detail, amount}
        table.insert(Entries, entry)
        local categoryNode = UpdateCategoryTree(entry)
        if treeviewCategory.SelectedNode == categoryNode then
            AddEntryToListView(entry)
        else
            treeviewCategory.SelectedNode = categoryNode
        end
    end
end

function FindCategoryNode(entry)
    local subject = entry[2]
    local category = entry[3]
    local subjectNode = subject == "outgo" and outgoNode or incomeNode
    local subNodes = subjectNode.Nodes.Find(category, false)
    if #subNodes == 0 then
        return nil, subjectNode
    else
        return subNodes[1], subjectNode
    end
end

function UpdateCategoryTree(entry)
    local categoryNode, subjectNode = FindCategoryNode(entry)
    if categoryNode == nil then
        local category = entry[3]
        categoryNode = subjectNode.Nodes.Add(category, category)
        categoryNode.Tag = {}
        dialog_Category.Items.Add(category)
    end
    table.insert(categoryNode.Tag, entry)
    return categoryNode
end

treeviewCategory.AfterSelect = function (sender, e)
    local entries = treeviewCategory.SelectedNode.Tag
    if entries ~= nil then
        listviewEntries.Items.Clear()
        for _,entry in ipairs(entries) do
            AddEntryToListView(entry)
        end
    end
end

function AddEntryToListView(entry)
    local item = WinForms.ListViewItem{ Text=entry[1] }
    item.SubItems.Add(entry[4])
    item.SubItems.Add(entry[5])
    item.Tag = entry
    listviewEntries.Items.Add(item)
end

function DeleteEntry()
    local item = listviewEntries.SelectedItems[1]
    local entry = item.Tag
    if entry ~= nil then
        local categoryNode = FindCategoryNode(entry)
        table.removeitem(categoryNode.Tag, entry)
        table.removeitem(Entries, entry)
        listviewEntries.Items.Remove(item)
    end
end

btnSave.Click = function (sender, e)
    local sfd = WinForms.SaveFileDialog{ Title="Save data", Filter="data file(*.dat)|*.dat" }
    if sfd.ShowDialog() == "OK" then
        local file = io.open(sfd.FileName, "w")
        file:write("Entries = {\r\n")
        for _,entry in ipairs(Entries) do
            file:write('{"', table.concat(entry, '", "'), '"},\r\n')
        end
        file:write('}')
        file:close()
    end
end

btnOpen.Click = function (sender, e)
    local ofd = WinForms.OpenFileDialog{ Title="Open data file", Filter="data file(*.dat)|*.dat" }
    if ofd.ShowDialog() == "OK" then
        dofile(ofd.FileName)
        incomeNode.Nodes.Clear()
        outgoNode.Nodes.Clear()
        dialog_Category.Items.Clear()
        for _,entry in ipairs(Entries) do
            UpdateCategoryTree(entry)
        end
        treeviewCategory.ExpandAll()
        listviewEntries.Items.Clear()
    end
end

WinForms.Run(form)