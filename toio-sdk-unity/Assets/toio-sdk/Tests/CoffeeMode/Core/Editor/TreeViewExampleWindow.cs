using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

// 親子構造を表現するためのモデルを定義しておく
// これがTreeViewに渡すモデルになる
public class ExampleTreeElement
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ExampleTreeElement Parent { get; private set; }
    private List<ExampleTreeElement> _children = new List<ExampleTreeElement>();
    public List<ExampleTreeElement> Children { get { return _children; } }

    /// <summary>
    /// 子を追加する
    /// </summary>
    public void AddChild(ExampleTreeElement child)
    {
        // 既に親がいたら削除
        if (child.Parent != null) {
            child.Parent.RemoveChild(child);
        }
        // 親子関係を設定
        Children.Add(child);
        child.Parent    = this;
    }

    /// <summary>
    /// 子を削除する
    /// </summary>
    public void RemoveChild(ExampleTreeElement child)
    {
        if (Children.Contains(child)) {
            Children.Remove(child);
            child.Parent        = null;
        }
    }
}

// TreeViewを表示するWindow
class TreeViewExampleWindow : EditorWindow
{
    // Stateはシリアライズする（Unity再起動しても状態を保持するため）
    [SerializeField]
    private TreeViewState _treeViewState;

    private ExampleTreeView _treeView;
    private SearchField _searchField;

    [MenuItem ("Window/Tree View Example")]
    private static void Open ()
    {
        GetWindow<TreeViewExampleWindow> (ObjectNames.NicifyVariableName(typeof(TreeViewExampleWindow).Name));
    }

    private void OnEnable ()
    {
        // Stateは生成されていたらそれを使う
        if (_treeViewState == null) {
            _treeViewState = new TreeViewState ();
        }

        // TreeViewを作成
        _treeView                                   = new ExampleTreeView(_treeViewState);
        // 親子関係を適当に構築したモデルを作成
        // IDは任意だが被らないように
        var currentId       = 0;
        var root            = new ExampleTreeElement { Id = ++currentId, Name = "1" };
        for (int i = 0; i < 2; i++) {
            var element     = new ExampleTreeElement { Id = ++currentId, Name = "1-" + (i + 1) };
            for (int j = 0; j < 2; j++) {
                element.AddChild(new ExampleTreeElement { Id = ++currentId, Name = "1-" + (i + 1) + "-" + (j + 1) });
            }
            root.AddChild(element);
        }
        // TreeViewを初期化
        _treeView.Setup(new List<ExampleTreeElement>{root}.ToArray());

        // SearchFieldを初期化
        _searchField                                = new SearchField();
        _searchField.downOrUpArrowKeyPressed        += _treeView.SetFocusAndEnsureSelectedItem;
    }

    private void OnGUI ()
    {
        // 検索窓を描画
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
            GUILayout.Space (100);
            GUILayout.FlexibleSpace();
            // TreeView.searchStringに検索文字列を入れると勝手に表示するItemを絞ってくれる
            _treeView.searchString                  = _searchField.OnToolbarGUI (_treeView.searchString);
        }

        // TreeViewを描画
        var rect    = EditorGUILayout.GetControlRect(false, 200);
        _treeView.OnGUI(rect);
    }
}

public class ExampleTreeView : TreeView
{
    private ExampleTreeElement[] _baseElements;

    public ExampleTreeView(TreeViewState treeViewState) : base(treeViewState) 
    {
        this.useScrollView = true;
    }

    public void Setup(ExampleTreeElement[] baseElements)
    {
        _baseElements       = baseElements;
        Reload();
    }

    protected override TreeViewItem BuildRoot ()
    {
        // BuildRootではRootだけを返す
        return new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
    }

    /// <summary>
    /// 選択されているものが切り替わった時の処理
    /// </summary>
    protected override void SelectionChanged (IList<int> selectedIds)
    {
        Debug.Log(selectedIds.Select(x => x.ToString()).Aggregate((a, b) => a + ", " + b));
    }

    protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
    {
        // 現在のRowsを取得
        var rows        = GetRows() ?? new List<TreeViewItem>();
        rows.Clear ();

        foreach (var baseElement in _baseElements)
        {
            var baseItem        = CreateTreeViewItem(baseElement);
            // Itemはrootとrowsの両方に追加していく
            root.AddChild (baseItem);
            rows.Add (baseItem);
            if (baseElement.Children.Count >= 1) {
                if (IsExpanded (baseItem.id))
                {
                    AddChildrenRecursive(baseElement, baseItem, rows);
                }
                else
                {
                    // 折りたたまれている場合はダミーのTreeViewItemを作成する（そういう決まり）
                    baseItem.children   = CreateChildListForCollapsedParent();
                }
            }
        }

        // depthを設定しなおす
        SetupDepthsFromParentsAndChildren(root);

        // rowsを返す
        return rows;
    }

    /// <summary>
    /// モデルとItemから再帰的に子Itemを作成・追加する
    /// </summary>
    private void AddChildrenRecursive (ExampleTreeElement element, TreeViewItem item, IList<TreeViewItem> rows)
    {
        foreach (var childElement in element.Children)
        {
            var childItem       = CreateTreeViewItem(childElement);
            item.AddChild (childItem);
            rows.Add (childItem);
            if (childElement.Children.Count >= 1) {
                if (IsExpanded (childElement.Id))
                {
                    AddChildrenRecursive(childElement, childItem, rows);
                }
                else
                {
                    // 折りたたまれている場合はダミーのTreeViewItemを作成する（そういう決まり）
                    childItem.children  = CreateChildListForCollapsedParent();
                }
            }
        }
    }

    /// <summary>
    /// ExampleTreeElementからTreeViewItemを作成する
    /// </summary>
    private TreeViewItem CreateTreeViewItem(ExampleTreeElement model)
    {
        return new TreeViewItem { id = model.Id, displayName = model.Name };
    }
}

/*
// 抽象クラスTreeViewを継承したクラスを作る
public class ExampleTreeView : TreeView
{
    private ExampleTreeElement[] _baseElements;

    public ExampleTreeView(TreeViewState treeViewState) : base(treeViewState)
    {
    }

    public void Setup(ExampleTreeElement[] baseElements)
    {
        // モデルを入れて
        _baseElements       = baseElements;
        // Reload()で更新（BuildRootが呼ばれる）
        Reload();
    }

    // ルートとなるTreeViewItemを作って返す
    // Reloadが呼ばれるたびに呼ばれる
    protected override TreeViewItem BuildRoot ()
    {
        // RootのItemはdepth = -1として定義する
        var root        = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

        //　モデルからTreeViewItemの親子関係を構築
        var elements    = new List<TreeViewItem>();
        foreach (var baseElement in _baseElements)
        {
            var baseItem        = CreateTreeViewItem(baseElement);
            root.AddChild(baseItem);
            AddChildrenRecursive(baseElement, baseItem);
        }

        // 親子関係に基づいてDepthを自動設定するメソッド
        SetupDepthsFromParentsAndChildren(root);

        return root;
    }

    /// <summary>
    /// モデルとItemから再帰的に子Itemを作成・追加する
    /// </summary>
    private void AddChildrenRecursive (ExampleTreeElement model, TreeViewItem item)
    {
        foreach (var childModel in model.Children)
        {
            var childItem       = CreateTreeViewItem(childModel);
            item.AddChild(childItem);
            AddChildrenRecursive(childModel, childItem);
        }
    }

    /// <summary>
    /// ExampleTreeElementからTreeViewItemを作成する
    /// </summary>
    private TreeViewItem CreateTreeViewItem(ExampleTreeElement model)
    {
        return new TreeViewItem { id = model.Id, displayName = model.Name };
    }
}
*/