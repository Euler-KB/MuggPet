# MuggPet.Xamarin.Android
This is the ultimate library aimed at improving development practices for building Xamarin.Android applications with minimal efforts.
**MuggPet** comes with a whole bunch of utilities including an advanced data binding framework, animation extensions , visual state manager, settings manager,generic adapter, credential store, asynchronous dialog and permission request patterns,commanding etc.

## Getting Started
Looking forward to getting that awesome application written with a whole bunch of existing functionalities? 
Then, **MuggPet** is definitely the best choice to get started with. 

### Dependencies
* Xamarin.Android.Support.V7.AppCompat
* Xamarin.Android.Support.V4
* Xamarin.Android.Support.Vector.Drawable
* Xamarin.Android.Support.Animated.Vector.Drawable

### **MuggPet Application**  
This starting point for utilizing various functionality of the library. It all starts by deriving from the BaseApplication class.
The base application class provides access to application lifecycle callbacks as events and also exposes a property for the active Activity. This enables other components to utilize application state without subclassing.

Below is a sample of application initialization
```C#
[Application(Label="@string/ApplicationName")]
public class MyApplication : BaseApplication
{
   protected MuggPetApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
   {

   }
        
   protected virtual void OnLoadComponents()
   {
      base.OnLoadComponents();
      
      //  TODO: Load other application components here
   }
}
```
In the above code, it is necessary to initialize other component within the 'OnLoadComponents' method. This ensures ordered initialization and prevents abnormal behaviours


Below are the lifecycle callbacks which are exposed by the BaseApplication as events  

+ ActivityStarted
+ ActivityStopped
+ ActivityDestroyed
+ ActivityResumed
+ ActivityPaused
+ ActivityRestarted
+ ActivityCreated
+ ActivitySaveInstanceState

```C#
//  Lifecycle handling example
BaseApplication.Instance.ActivityCreated += (activity,bundle) =>
{
  Log.Debug("MuggPet.App",$"{activity.GetType().Name} has been created!");
};
```

### Activity
MuggPet enhances android activities way much more than as it seems. All you have to do is simply derive your activities from BaseActivity and let MuggPet simplify your operations.


```C#
[Activity(MainLauncher=true)]
public class MainActivity : BaseActivity
{
    //...
}
```
MuggPet base activity implements the core of various functionalities including support for data binding, visual state manager, async activity starting and permission request.

BaseActivity can simplify the process involved in inflating and binding views in your activity.  
Below is an example.

```C#
[Activity(MainLauncher=true)]
public class MainActivity : BaseActivity
{
   public MainActivity(): base(Resource.Layout.Main)
   {
    
   }
   
   protected overide void OnLoaded()
   {
      //  TODO: Implement activity loaded logic here
   }
}
```
The code above simplifies the process of setting the content view of the activity and automatically binds (attaches) views within the activity.
This introduces a new concept termed as 'Binding'
### Binding
Binding with MuggPet can be defined by its various operations
+ **View Attachment**  
A view attachment binding is simply an attachment of a view to a property within an object such as an activity. Those familiar with **Butterknife** already should have fair idea about this mode of binding does.
This type of binding is executed by reflecting the all members of the source object which are adorned with BindIDAttribute.
If any is found, the adorned member is assigned the view with the id specified.

A typical example is below
```C#
[Activity(MainLauncher=true)]
public class MainActivity : BaseActivity
{
   [BindID(Resource.Id.myButton)]
   Button myButton = null;

   public MainActivity(): base(Resource.Layout.Main)
   {
    
   }
   
   protected overide void OnLoaded()
   {
      //  TODO: Implement activity loaded logic here
      //  We are safe to access 'myButton'. Thanks to MuggPet's binding engine (:-
      myButton.Click += (s,e) =>
      {
          Toast.MakeText(this, "Clicked me!", ToastLength.Short).Show();
      };
   }
}
```
Here, '*myButton*' was assigned upon binding (view attachment).
It is important to note that the type adorned member should either be a subclass or the same type of the view to be attached.

+ **Command Binding**  
This mode of binding is simply defined by routing an event of a view to a command object(target).
A simple scenario is demonstrated below

```C#

[Activity(MainLauncher=true)]
public class MainActivity : BaseActivity
{

   private ICommand clickCommand;
   
   [BindCommand(Resource.Id.myButton)]
   public ICommand ClickCommand
   {
      get
      {
        return clickCommand ?? (clickCommand = new RelayCommand((args) =>
        {
            Toast.MakeText(this, "Clicked me!", ToastLength.Short).Show();
        }))
      }
   }

   public MainActivity(): base(Resource.Layout.Main)
   {
    
   }
  
}

```
From the code above, the click command is simply routed to the Click event of the button with the id '*myButton*'.  
**Note**: A command binding involves the use of **BindCommandAttribute** not *BindIDAttribute*.
This attribute ensures the appropriate behaviour of commands.
The relay command is exposes a callback for defining the execute action for the command.

+ **Resource Binding**  
This mode of binding involves the loading and assigning of resources to object members.

```C#
[Activity(MainLauncher=true)]
public class MainActivity : BaseActivity
{
    [BindResource(Resource.String.ApplicationName)]
    public string ApplicationName;
    
    //... code omitted for brevity
    
    //...
    
    protected overide void OnLoaded()
    {
        Log.Debug("MuggPet.App",ApplicationName);
    }
}
```
This code above fetches the resource string with id 'ApplicationName' and assigns it to the variable 'ApplicationName' within the activity. 
**Tip**: The resource binding operation automatically determines the type of resource to load by the type of the member adorned.

```C#
// examples of clever resource loading

//  This will load an integer resource
[BindResource(Resource.Integer.ApplicationName)]
int myvar;


```

At this point, you should probably be wondering how binding works under the hood.
If you did, then you just got lucky.

#### Introducing the BindingHandler  
The BaseActivity class implements an 'ISupportBinding' interface which basically exposes the 'IBindingHandler' interface.
The binding handler is the core of binding for any object that wishes to it. The binding handler in addition posses a resource manager
for caching views and resources for quicker usage.

So, here's how BaseActivity does the magic

```C#
class BaseActivity : ISupportBinding //,... And a whole bunch of other interfaces (:-
{
   private IBindingHandler bindingHandler;
   public IBindingHandler BindingHandler
   {
      get
      {
        return bindingHandler ?? (bindingHandler = new BindingHandler());
      }
   }
   
   // ... other implementations not included
}

```

Ok, I know you're understanding the show a bit. But where does the binding take place?


