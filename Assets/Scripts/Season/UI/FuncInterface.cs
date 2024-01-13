namespace Season.UI
{
    //Set to btn config, will be automatically bind to that btn
    public interface IOpenWidget
    {
        void OpenWidget();
    }
    public interface ICloseWidget
    {
        
        void Deactivated();
        void CloseWidget();
    }
    
    public interface IInitWidget
    {
        void InitializeUiItem();
    }
    
    // simulate btn 
    public interface ITickWhenOpen
    {
        void TickWhenOpen();
    }
}