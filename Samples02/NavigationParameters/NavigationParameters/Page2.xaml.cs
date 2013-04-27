using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Navigation;

namespace NavigationParameters
{
    public partial class Page2 : PhoneApplicationPage
    {
        private ObservableCollection<string> coffees =
            new ObservableCollection<string>();
        private ObservableCollection<string> teas =
            new ObservableCollection<string>();

        public Page2()
        {
            InitializeComponent();

            coffees.Add("Blue Mountain");
            coffees.Add("Monsooned Malabar");
            coffees.Add("Sumatra");
            coffees.Add("Monkey Poo");
            coffees.Add("Tanzania Peaberry");

            teas.Add("Earl Grey");
            teas.Add("Darjeeling");
            teas.Add("Jasmine");
            teas.Add("Oolong");
            teas.Add("Chrysanthemum");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string drinkType;
            if (NavigationContext.QueryString.TryGetValue(
                "drink", out drinkType))
            {
                switch (drinkType)
                {
                    case "Coffee":
                        SetItemsCoffee();
                        break;
                    case "Tea":
                        SetItemsTea();
                        break;
                }
            }
        }

        protected override void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
            switch (e.Fragment)
            {
                case "Coffee":
                    SetItemsCoffee();
                    break;
                case "Tea":
                    SetItemsTea();
                    break;
            }
        }

        private void SetItemsCoffee()
        {
            listDrinks.ItemsSource = coffees;
            pageTitle.Text = "coffee";
            pageTitle.Foreground = listDrinks.Foreground 
                = new SolidColorBrush(Colors.Brown);
        }

        private void SetItemsTea()
        {
            listDrinks.ItemsSource = teas;
            pageTitle.Text = "tea";
            pageTitle.Foreground = listDrinks.Foreground
                = new SolidColorBrush(Colors.Green);
        }
    }
}