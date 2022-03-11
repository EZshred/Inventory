using FreshMvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Inventory
{
    public class ItemPageModel : FreshBasePageModel
    {
        // Use IoC to get our repository.
        private Repository _repository = FreshIOC.Container.Resolve<Repository>();

        // Backing data model.
        private Item _item;

        /// <summary>
        /// Public property exposing the item's name for Page binding.
        /// </summary>
        public string ItemName
        {
            get { return _item.Name; }
            set { _item.Name = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// Public property exposing the item's barcode for Page binding.
        /// </summary>
        public string ItemBarcode
        {
            get { return _item.Barcode; }
            set { _item.Barcode = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// Public property exposing the item's quantity for Page binding.
        /// </summary>
        public int ItemQuantity
        {
            get { return _item.Quantity; }
            set { _item.Quantity = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// Called whenever the page is navigated to.
        /// Either use a supplied Intem, or create a new one if not supplied.
        /// FreshMVVM does not provide a RaiseAllPropertyChanged,
        /// so we do this for each bound property, room for improvement.
        /// </summary>
        public override void Init(object initData)
        {
            _item = initData as Item;
            if (_item == null) _item = new Item();
            base.Init(initData);
            RaisePropertyChanged(nameof(ItemName));
            RaisePropertyChanged(nameof(ItemBarcode));
        }

        /// <summary>
        /// Command associated with the save action.
        /// Persists the item to the database if the item is valid.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (_item.IsValid())
                    {
                        await _repository.CreateItem(_item);
                        await CoreMethods.PopPageModel(_item);
                    }
                });
            }
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            var scanner = FreshIOC.Container.Resolve<IScanner>();

            scanner.Enable();
            scanner.OnScanDataCollected += ScannedDataCollected;

            var config = new ZebraScannerConfig();
            config.IsUPCE0 = false;
            config.IsUPCE1 = false;

            scanner.SetConfig(config);
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            var scanner = FreshIOC.Container.Resolve<IScanner>();

            if (null != scanner)
            {
                scanner.Disable();
                scanner.OnScanDataCollected -= ScannedDataCollected;
            }
            base.ViewIsDisappearing(sender, e);
        }


        private void ScannedDataCollected(object sender, StatusEventArgs a_status)
        {
            Barcode barcode = new Barcode();
            barcode.Data = a_status.Data;
            barcode.Type = a_status.BarcodeType;

            ItemBarcode = barcode.Data;
        }
    }
}
