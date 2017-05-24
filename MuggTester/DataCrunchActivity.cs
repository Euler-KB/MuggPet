using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MuggPet.Binding;
using MuggPet.Assets;
using System.Xml.Linq;
using Android.Graphics;
using MuggPet.Commands;
using MuggPet.App;
using MuggPet.Dialogs;
using Android.Util;
using MuggPet.Views;
using MuggPet.Views.VisualState;
using MuggPet.App.Activity.Attributes;
using MuggPet.Adapters;
using MuggTester.Extensibility;

namespace MuggTester
{
    [Activity(Label = "Data Crunch", Theme = "@style/AppLightTheme", MainLauncher = true)]
    public class DataCrunchActivity : MuggPet.App.Activity.BaseActivity
    {
        public enum CrunchModelType
        {
            Data,
            GroupHeader
        }

        public class ModelBase : ViewModelBase
        {
            public CrunchModelType ModelType { get; }

            public ModelBase(CrunchModelType type)
            {
                ModelType = type;
            }

            public int GetItemLayout()
            {
                switch (ModelType)
                {
                    case CrunchModelType.Data:
                        return Resource.Layout.crunch_item_layout;
                    case CrunchModelType.GroupHeader:
                        return Resource.Layout.crunch_group_layout;
                    default:
                        return 0;
                }
            }

        }

        public class Crunch : ModelBase
        {
            string header;
            string message;
            DateTime date;
            bool accepted;
            float seekValue;

            public Crunch(string header, string message, DateTime date, bool accepted) : base(CrunchModelType.Data)
            {
                this.header = header;
                this.message = message;
                this.date = date;
                this.accepted = accepted;
            }

            [BindID(Resource.Id.lbHeader)]
            public string Header
            {
                get { return header; }
                set { Set(ref header, value); }
            }

            [BindID(Resource.Id.lbMessage)]
            public string Message
            {
                get { return message; }
                set { Set(ref message, value); }
            }

            [BindID(Resource.Id.lbDate, StringFormat = "Processed: {0:g}")]
            public DateTime Time
            {
                get { return date; }
                set { Set(ref date, value); }
            }

            [BindColor(Resource.Id.lbState, Mode = ColorSetMode.Foreground)]
            public Color StatusColor => accepted ? Color.Green : Color.Red;

            [BindID(Resource.Id.lbState)]
            string Status => accepted ? "Accepted" : "Declined";

            [BindID(Resource.Id.btnProcessRequest)]
            public string RequestLabel => accepted ? "Decline Request" : "Accept Request";

            [BindCommand(Resource.Id.btnDelete, Tag = "Delete item")]
            public ICommand DeleteCommand
            {
                get
                {
                    return ((DataCrunchActivity)BaseApplication.CurrentActivity).DeleteCommand;
                }
            }

            [BindID(Resource.Id.seekRange, Target = "Progress")]
            [PropertyLink(nameof(CardBackground))]
            public float SeekValue
            {
                get { return seekValue; }
                set { Set(ref seekValue, value); }
            }

            [BindID(BindConsts.RootViewId, Target = "SetCardBackgroundColor")]
            public int CardBackground
            {
                get
                {
                    float p = SeekValue / 100F;
                    Color start = Color.White;
                    Color end = Color.Orange;
                    return new Color(
                        (byte)(start.R + (end.R - start.R) * p),
                        (byte)(start.G + (end.G - start.G) * p),
                        (byte)(start.B + (end.B - start.B) * p),
                        (byte)(start.A + (end.A - start.A) * p)).ToArgb();
                }
            }

            [BindCommandEx(Resource.Id.seekRange)]
            static ICommand seekChangedCmd;

            [BindCommand(Resource.Id.btnProcessRequest, Tag = "Process Request")]
            static ICommand reqCmd;

            static Crunch()
            {
                seekChangedCmd = new RelayCommand((args) =>
                {
                    var cmd = args as CommandArgs;
                    if (cmd != null)
                    {
                        Crunch crunch = (Crunch)cmd.Parameter;
                        crunch.SeekValue = cmd.Progress;
                    }
                });

                reqCmd = new RelayCommand(async (args) =>
                {
                    var item = args as Crunch;
                    if (item.accepted)
                    {
                        if (await CommonDialogs.ShowAcceptDialog(BaseApplication.CurrentActivity, "Decline Request",
                             $"\"{item.header}\"{System.Environment.NewLine}Are you sure you want to decline request?"))
                        {
                            //  decline
                            item.date = DateTime.Now;
                            item.Accepted = false;
                            ((DataCrunchActivity)(BaseApplication.CurrentActivity)).UpdateItemGroup(item);
                        }
                    }
                    else
                    {
                        if (await CommonDialogs.ShowAcceptDialog(BaseApplication.CurrentActivity, "Accept Request!",
                             $"\"{item.header}\"{System.Environment.NewLine}Are you sure you want to accept request?"))
                        {
                            //  accept
                            item.date = DateTime.Now;
                            item.Accepted = true;
                            ((DataCrunchActivity)(BaseApplication.CurrentActivity)).UpdateItemGroup(item);
                        }
                    }
                });
            }

            [BindVisibility(Resource.Id.lbTip, Invert = true)]
            [PropertyLink(nameof(Status), nameof(StatusColor), nameof(RequestLabel), nameof(Time))]
            public bool Accepted
            {
                get { return accepted; }
                set { Set(ref accepted, value); }
            }
        }

        public class GroupInfo : ModelBase
        {
            public bool Accepted { get; set; }

            [BindID(Resource.Id.lbGroupHeader)]
            public string GroupHeader
            {
                get
                {
                    return Accepted ? "Accepted" : "Declined";
                }
            }

            [BindID(Resource.Id.imgGroupLogo)]
            public int GroupIconResId { get; set; }

            public GroupInfo(bool accepted) : base(CrunchModelType.GroupHeader)
            {
                Accepted = accepted;
            }

            public override string ToString()
            {
                return GroupHeader;
            }
        }

        [BindID(Resource.Id.lvCrunch)]
        private ListView lstView = null;

        private GenericAdapter<ModelBase> crunchAdapter;

        private ObservableCollection<ModelBase> dataSource;

        IEnumerable<ModelBase> GroupItems(IEnumerable<Crunch> source)
        {
            //
            List<ModelBase> items = new List<ModelBase>();
            foreach (var group in source.GroupBy(x => x.Accepted))
            {
                items.Add(new GroupInfo(group.Key) { GroupIconResId = Resource.Drawable.Icon });
                items.AddRange(group.OrderBy(x => x.Header));
            }

            return items;
        }

        Task LoadDataSource()
        {
            return Task.Run(() =>
            {
                //  you could also fetch data from a web service/api
                var doc = XDocument.Load(new System.IO.StringReader(Assets.ReadString("CrunchDataPart.xml")));
                var source = doc.Descendants("Crunch").Select(x => new Crunch(
                        x.Element("Header")?.Value,
                        x.Element("Message")?.Value,
                        DateTime.Parse(x.Element("Date")?.Value),
                        bool.Parse(x.Element("Accepted")?.Value)));

                //
                dataSource = new ObservableCollection<ModelBase>(GroupItems(source));
            });
        }

        ICommand deleteCmd;
        public ICommand DeleteCommand
        {
            get
            {
                return deleteCmd ?? (deleteCmd = new RelayCommand(async (args) =>
                {
                    Crunch item = args as Crunch;
                    if (item != null)
                    {
                        if (await this.ShowAcceptDialog($"Delete \"{item.Header}\"", "Are you sure you want to delete that crunch?"))
                            dataSource.Remove(item);
                    }
                }));
            }
        }

        public void UpdateItemGroup(Crunch crunch)
        {
            var groupInfo = dataSource.Where(x => x.ModelType == CrunchModelType.GroupHeader)
                .Select(x => new { Key = ((GroupInfo)x).Accepted, Index = dataSource.IndexOf(x) });

            var alternate = groupInfo.First(x => x.Key == crunch.Accepted);
            dataSource.Move(dataSource.IndexOf(crunch), alternate.Index + 1);
        }

        protected override void OnDefineVisualStates()
        {
            VisualState.RegisterState("Normal", defaultState: true);

            VisualState.RegisterState("Open")
                        .OnActivate((state) =>
                        {
                            state.HasView(Resource.Id.coverView)
                                 .TranslateY(-this.Resources.GetDimension(Resource.Dimension.cover_height));

                            state.HasView(Resource.Id.tbUsername)
                                    .Enable();

                            state.HasView(Resource.Id.tbPassword)
                                .Enable();
                        });

        }

        [MenuAction(Resource.Id.activate_open_state)]
        public void ActivateOpenState()
        {
            VisualState.GotoState("Open");
        }

        [MenuAction(Resource.Id.activate_default_state)]
        public void ActivateDefault()
        {
            VisualState.GotoDefaultState();
        }

        ICommand showCommand;
        public ICommand ShowCommand
        {
            get
            {
                return showCommand ?? (showCommand = new RelayCommand(async (args) =>
                {
                    //
                    await CommonDialogs.ShowMessage(this, "Hello", "The show command just executed! YAY!!!", "Love it", false);

                }));
            }
        }

        public DataCrunchActivity() : base(Resource.Layout.crunch_layout_main, menu: Resource.Menu.crunch_menu)
        {

        }

        protected override async void OnLoaded()
        {
            //
            AttachSupportToolbar(Resource.Id.support_toolbar);

            //
            await LoadDataSource();

            //  create adapter
            crunchAdapter = new GenericAdapter<ModelBase>(this, dataSource, (item) => item.GetItemLayout());
            crunchAdapter.CanReuseView += (item, view, original) =>
            {
                return item?.ModelType == original?.ModelType;
            };


            //
            lstView.Adapter = crunchAdapter;
        }
    }
}