using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace NetPackUpClient {
    public partial class MainWindowView : Window {
        private int tabCount = 1;

        public MainWindowView() {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            var stackPanel = button.Parent as StackPanel;
            var textBox = stackPanel.Children[0] as TextBox;
            var link = textBox.Text;

            if (!string.IsNullOrWhiteSpace(link)) {
                var grid = stackPanel.Parent as Grid;
                var treeView = grid.Children[1] as TreeView;
                LoadSharedDirectory(link, treeView);
            } else {
                MessageBox.Show("请输入有效的链接。");
            }
        }

        private void LoadSharedDirectory(string link, TreeView treeView) {
            try {
                var rootDirectoryInfo = new DirectoryInfo(link);
                var sharedDirectory = new SharedRemoteDirectory(
                    link,
                    "Connected",
                    rootDirectoryInfo
                );
                treeView.Items.Clear();
                treeView.Items.Add(CreateDirectoryNode(sharedDirectory.DirectoryInfo));
            } catch (Exception ex) {
                MessageBox.Show($"无法加载共享目录: {ex.Message}");
            }
        }

        private TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo) {
            var directoryNode = new TreeViewItem { Header = directoryInfo.Name, Tag = directoryInfo };
            foreach (var directory in directoryInfo.GetDirectories()) {
                directoryNode.Items.Add(CreateDirectoryNode(directory));
            }
            foreach (var file in directoryInfo.GetFiles()) {
                var fileNode = new TreeViewItem { Header = file.Name, Tag = file };
                directoryNode.Items.Add(fileNode);
            }
            return directoryNode;
        }

        private void SharedTreeView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            HandleMouseDoubleClick(sender as TreeView);
        }

        private void HandleMouseDoubleClick(TreeView treeView) {
            if (treeView.SelectedItem is TreeViewItem selectedItem) {
                if (selectedItem.Tag is DirectoryInfo directoryInfo) {
                    // Process.Start("explorer.exe", directoryInfo.FullName);
                } else if (selectedItem.Tag is FileInfo fileInfo) {
                    // Process.Start(new ProcessStartInfo(fileInfo.FullName) { UseShellExecute = true });
                } else {
                    MessageBox.Show("无法打开文件或文件夹。");
                }
            }
        }

        private void TabControlMain_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (TabControlMain.SelectedIndex == TabControlMain.Items.Count - 1) {
                // Do nothing, the add button is handled by a different event
            }
        }

        private void AddTabButton_Click(object sender, RoutedEventArgs e) {
            if (tabCount < 9) {
                AddNewTab();
                tabCount++;
            }
        }

        private void AddNewTab() {
            var newTab = new TabItem {
                Header = $"Tab {tabCount + 1}",
                Content = CreateTabContent()
            };
            TabControlMain.Items.Insert(TabControlMain.Items.Count - 1, newTab);
            TabControlMain.SelectedIndex = TabControlMain.Items.Count - 2;
        }

        private Grid CreateTabContent() {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10) };
            var textBox = new TextBox { Width = 300, Margin = new Thickness(0, 0, 10, 0) };
            var button = new Button { Content = "连接" };
            button.Click += ConnectButton_Click;
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(button);

            var treeView = new TreeView { Margin = new Thickness(10) };

            grid.Children.Add(stackPanel);
            grid.Children.Add(treeView);
            Grid.SetRow(treeView, 1);

            return grid;
        }
    }

    public class SharedRemoteDirectory {
        public string Link { get; set; }
        public string Status { get; set; }
        public DirectoryInfo DirectoryInfo { get; set; }
        public DateTime ConnectionTime { get; set; }

        public SharedRemoteDirectory(string link, string status, DirectoryInfo directoryInfo) {
            Link = link;
            Status = status;
            DirectoryInfo = directoryInfo;
            ConnectionTime = DateTime.Now;
        }
    }
}