using Android.App;
using Android.Widget;
using Android.OS;
using System.Linq;
using MuggPet;
using MuggPet.Binding;
using MuggPet.Utils;
using System;
using Android.Speech.Tts;
using Android.Runtime;
using Java.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.Views;
using MuggPet.Commands;
using MuggPet.Activity.Attributes;
using MuggPet.Utils.Adapter;

namespace MuggTester
{
    [Activity(Label = "Mugg-Tester", MainLauncher = false, Icon = "@drawable/icon", Theme = "@style/AppTheme")]
    public class MainActivity : MuggPet.Activity.AppActivityBase
    {
        public static string[] Colors = new string[]
        {
            "Red",
            "Green",
            "Ash",
            "Grey",
            "Pink",
            "Blue",
            "Violet"
        };

        public static string[] Models = new string[]
        {
            "Opel Astra",
            "Benz",
            "BMW",
            "Kia",
            "Ford",
            "Toyota",
            "Porshe",
            "Gallardo",
            "Vauxhall",
            "BMW B"
        };

        public class Car
        {
            [BindID(Resource.Id.car_model)]
            public string Model { get; set; }

            [BindID(Resource.Id.car_engine_capacity, StringFormat = "Engine Capacity: {0:F2}")]
            public decimal EngineCapacity { get; set; }

            [BindID(Resource.Id.car_color, StringFormat = "Color: {0}")]
            public string Color { get; set; }
        }

        [BindID(Resource.Id.btnClick)]
        Button btnClick = null;

        [BindID(Resource.Id.btnGenerateRandom)]
        Button btnGenerateRandom = null;

        [BindID(Resource.Id.lbTotalItems)]
        TextView lbTotalItems = null;

        [BindID(Resource.Id.searchBox)]
        SearchView searchBox = null;

        [BindAdapter(Resource.Id.mySpinner, StringFormat = "Country Name: {0}", ItemsSource = new string[]
        {
            "Ghana",
            "America",
            "Togo",
            "Brazil"
        })]
        [SortDescription(Mode = SortOrder.Descending)]
        Spinner mySpinner = null;

        [BindAdapter(Resource.Id.myList, ItemsResourceId = Resource.Array.MyItems)]
        [SortDescription(Mode = SortOrder.Ascending)]
        ListView myList;

        //RelayCommand showSettingsCommand;

        [MenuAction(Resource.Id.settings_action)]
        void ShowSettings(IMenuItem item)
        {
            Navigate(typeof(MuggPreferenceScreen));
        }

        RelayCommand generateCommand;

        [BindID(Resource.Id.btnGenerateRandom)]
        RelayCommand GenerateRandom
        {
            get
            {
                return generateCommand ?? (generateCommand = new RelayCommand((args) =>
                {
                    return MuggPet.Dialogs.CommonDialogs.ShowMessage(this, "Generate Command", "Yeah, you clicked it!");
                }));
            }
        }

        static string[] myDataSource = new string[]
        {
            "Ghana",
            "Togo",
            "Nigeria",
            "Burkina Faso",
            "England",
            "Mockup"
        };

        //[BindAdapter(Resource.Id.myList, ItemLayout = Resource.Layout.car_item_layout)]
        //[SortDescription(nameof(Car.EngineCapacity), Mode = ComparisonMode.Descending)]
        //private ObservableCollection<Car> carsDataSource;

        System.Random random = new System.Random();

        public MainActivity() : base(Resource.Layout.Main, menuResourceID: Resource.Menu.menu_main, closeMethod: CloseMethod.System)
        {
            LoadDataSource();
        }

        void LoadDataSource()
        {
            // load data source
            List<Car> source = new List<Car>();
            for (int i = 0; i < 2500; i++)
            {
                source.Add(new Car()
                {
                    Color = Colors[random.Next(Colors.Length)],
                    EngineCapacity = (decimal)(random.NextDouble() * 300),
                    Model = Models[random.Next(Models.Length)]
                });
            }

            //
            //carsDataSource = new ObservableCollection<Car>(source);
        }

        protected override void OnLoaded()
        {
            //  create suppor toolbar here
            AttachSupportToolbar();

            //  bind all 
            this.BindObjectToView(this);

            //
            //var adapter = ((GenericAdapter<Car>)myList.Adapter);
            //adapter.Filter += (item) =>
            //{
            //    if (searchBox.Query.Length == 0)
            //        return true;

            //    return item.Model.IndexOf(searchBox.Query, StringComparison.CurrentCultureIgnoreCase) >= 0;
            //};

            //btnClick.Click += (s, e) =>
            //{
            //    adapter.Refresh();
            //};

            //btnGenerateRandom.Click += (s, e) =>
            //{
            //    carsDataSource.Add(new Car()
            //    {
            //        Color = Colors[random.Next(Colors.Length)],
            //        EngineCapacity = (decimal)(random.NextDouble() * 300),
            //        Model = Models[random.Next(Models.Length)]
            //    });
            //};

            ////
            //searchBox.QueryTextChange += (s, e) => adapter.Refresh();
            //searchBox.QueryTextSubmit += (s, e) => adapter.Refresh();

            ////
            //adapter.Changed += (s, e) =>
            //{
            //    lbTotalItems.Text = $"Total items: {adapter.Count}";
            //};

            //myList.ItemClick += (s, e) =>
            //{

            //};
        }


    }
}

