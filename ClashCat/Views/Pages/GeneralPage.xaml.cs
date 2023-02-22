using ClashCat.Helpers;
using ClashCat.Models.Clash.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Windows.Documents;
using Wpf.Ui.Common.Interfaces;

namespace ClashCat.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class GeneralPage : INavigableView<ViewModels.GeneralViewModel>
    {
        public ViewModels.GeneralViewModel ViewModel { get; }

        public GeneralPage(ViewModels.GeneralViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();
        }

        private async void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.PopupShow ^= true;
            await qwq.ShowAndWaitAsync();
        }

        private void TestButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string yaml = YamlHelper.Serializer.Serialize(new ClashConfig());
            var config = YamlHelper.Deserializer.Deserialize<ClashConfig>(yaml);

            File.WriteAllText("test.yaml", yaml);
        }

        [YamlAbstract(typeof(Student), typeof(Teacher))]
        abstract class Person
        {
            public abstract string Type { get; }
        }

        [YamlVariant(nameof(Type), "Student")]
        class Student : Person
        {
            public override string Type => "Student";
        }

        [YamlVariant(nameof(Type), "Teacher")]
        class Teacher : Person
        {
            public override string Type => "Teacher";
        }
    }
}