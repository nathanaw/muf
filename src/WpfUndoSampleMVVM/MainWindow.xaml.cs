using System.Windows;

namespace WpfUndoSampleMVVM
{

    // ********************************************************************************************************************
    // NOTE:
    //  For this sample, the window is the "model" and also the "root" of the undo document hierarchy.
    //  As a result, this window is used for bindings and therefor must support INotifyPropertyChanged and ISupportsUndo.
    // 
    // In a production application, using the code-behind like this is probably not the "best practice". 
    // However, for a sample, it simplifies the number of concepts needed to understand how the undo system works.
    // ********************************************************************************************************************
    public partial class MainWindow 
        : Window 
    {

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion
        
        #region Event Handlers

        #endregion



    }
}
