using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerServiceSteps
{
    public class ItemsSummery
    {
        public UInt64 NumberOfTotalItemsEnequed;
        public UInt64 NumberOfItemsSuccessfullyProcessed;
        public UInt64 NumberOfItemsFailedToProcess;

        private static ItemsSummery _itemsSummery = new ItemsSummery();
        private static object _object = new object();

        private ItemsSummery()
        { }

        public static UInt64 GetNumberOfTotalItemsEnequed()
        {
            lock (_object)
            {
                return _itemsSummery.NumberOfTotalItemsEnequed;
            }
        }
        public static UInt64 GetNumberOfItemsSuccessfullyProcessed()
        {
            lock (_object)
            {
                return _itemsSummery.NumberOfItemsSuccessfullyProcessed;
            }
        }
        public static UInt64 GetNumberOfItemsFailedToProcess()
        {
            lock (_object)
            {
                return _itemsSummery.NumberOfItemsFailedToProcess;
            }
        }

        public static UInt64 IncrementNumberOfTotalItemsEnequed()
        {
            lock (_object)
            {
                _itemsSummery.NumberOfTotalItemsEnequed++;
                return _itemsSummery.NumberOfTotalItemsEnequed;
            }
        }
        public static UInt64 IncrementNumberOfItemsSuccessfullyProcessed()
        {
            lock (_object)
            {
                _itemsSummery.NumberOfItemsSuccessfullyProcessed++;
                return _itemsSummery.NumberOfItemsSuccessfullyProcessed;
            }
        }
        public static UInt64 IncrementNumberOfItemsFailedToProcess()
        {
            lock (_object)
            {
                _itemsSummery.NumberOfItemsFailedToProcess++;
                return _itemsSummery.NumberOfItemsFailedToProcess;
            }
        }

        public static bool AreAllItemsProcessed()
        {
            lock (_object)
            {
                return _itemsSummery.NumberOfTotalItemsEnequed == (_itemsSummery.NumberOfItemsSuccessfullyProcessed + _itemsSummery.NumberOfItemsFailedToProcess);
            }
        }
    }
}
