﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;


namespace Jamie.Model
{
    public class ShoppingList
    {

    }

    public class ShoppingListSet
    {

    }

    public class ShoppingListItem
    {
        //static Variables
        private static IngredientSet _IngredientSetData;
        private static UnitSet _UnitSetData;
        private static UnitTranslationSet _UnitTranslationSetData;

        //Variables
        private long? _ID;
        private Ingredient _Ingredient;
        private double _IngredientUnitQuantity;
        private double _Quantity;
        private Unit _Unit;
        private FoodPlanItem _ReferredFoodPlanItem;
        


        //Constructors
        public ShoppingListItem()
        {
        }

        //Properties
        public long? ID
        {
            get
            {
                return _ID;
            }

            set
            {
                _ID = value;
            }
        }
        public Ingredient Ingredient
        {
            get
            {
                return _Ingredient;
            }

            set
            {
                _Ingredient = value;
            }
        }
        public static IngredientSet IngredientSetData
        {
            get
            {
                return _IngredientSetData;
            }
        } //Readonly
        public double Quantity
        {
            get
            {
                return _Quantity;
            }

            set
            {
                _Quantity = value;
            }
        }
        public FoodPlanItem ReferredFoodPlanItem
        {
            get
            {
                return _ReferredFoodPlanItem;
            }

            set
            {
                _ReferredFoodPlanItem = value;
            }
        }
        public Unit Unit
        {
            get
            {
                return _Unit;
            }

            set
            {
                _Unit = value;
            }
        }
        public static UnitSet UnitSetData
        {
            get
            {
                return _UnitSetData;
            }
        } //Readonly
        public static UnitTranslationSet UnitTranslationSetData
        {
            get
            {
                return _UnitTranslationSetData;
            }
        }//Readonly


        //Methods
        public void CalculateTargetUnit()
        {
            _IngredientUnitQuantity = Quantity * UnitTranslationSetData.GetTranslation(Unit, Ingredient.TargetUnit, Ingredient).TranslationFactor;
            // hier könnte von GetTranslation auch null zurückgeliefert werden... ????
        }
        public bool Equals(ShoppingListItem ItemToCompare)
        {
            if (ItemToCompare == null) return false;
            return ID.Equals(ItemToCompare.ID) || EqualKey(ItemToCompare);
        }
        public bool EqualKey(ShoppingListItem ItemToCompare)
        {
            //            return Symbol.Equals(ItemToCompare.Symbol);
            return false; //muss noch angepasst werden.
        }
        public void SetDataReference(IngredientSet IngredientSetData, UnitSet UnitSetData, UnitTranslationSet UnitTranslationSetData)
        {
            _IngredientSetData = IngredientSetData;
            _UnitSetData = UnitSetData;
            _UnitTranslationSetData = UnitTranslationSetData;
        }
        public override string ToString()
        {
            string ReturnString;

            ReturnString = string.Format("{0,6} {1,10:N2} {2,10} {3,15} {4,10:N2} {5,10}", ID, Quantity, Unit.Symbol, Ingredient.Name, _IngredientUnitQuantity, Ingredient.TargetUnit.Symbol);
            if (ReferredFoodPlanItem != null) ReturnString += string.Format(" Recipe: {0,20} - {1:d}, {2}",ReferredFoodPlanItem.PlannedRecipe.Name, ReferredFoodPlanItem.DateToConsume, ReferredFoodPlanItem.DateToConsume.DayOfWeek);
            return ReturnString;
        }
    }

    public class ShoppingListItemSet : ObservableCollection<ShoppingListItem>
    {
        //Constants
        private const string FileExtension = ".shli";// --> Data

        //static Variables
        private static long _MaxID = 0;
        private static IngredientSet _IngredientSetData;
        private static UnitSet _UnitSetData;
        private static UnitTranslationSet _UnitTranslationSetData;


        //Variables
        private DateTime _DueDate;
        private string _Name;
        private string _Responsible;
        private string _Shop;

        private ShoppingListItem _SelectedItem;

        //Constructors
        public ShoppingListItemSet()
        {         
        }

        //Properties
        public DateTime DueDate
        {
            get
            {
                return _DueDate;
            }

            set
            {
                _DueDate = value;
            }
        }
        public static IngredientSet IngredientSetData
        {
            get
            {
                return _IngredientSetData;
            }
        }//Readonly
        public static long MaxID
        {
            get
            {
                return _MaxID;
            }
        } //Readonly
        public string Name
        {
            get
            {
                return _Name;
            }

            set
            {
                _Name = value;
            }
        }
        public string Responsible
        {
            get
            {
                return _Responsible;
            }

            set
            {
                _Responsible = value;
            }
        }
        public ShoppingListItem SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
        } //Readonly
        public string Shop
        {
            get
            {
                return _Shop;
            }

            set
            {
                _Shop = value;
            }
        }
        public static UnitSet UnitSetData
        {
            get
            {
                return _UnitSetData;
            }
        } //Readonly
        public static UnitTranslationSet UnitTranslationSetData
        {
            get
            {
                return _UnitTranslationSetData;
            }
        } //Readonly

        //Methods
        public bool AddItem(ShoppingListItem ItemToBeAdded)
        {
            if (!Contains(ItemToBeAdded))
            {
                ItemToBeAdded.ID = ++_MaxID;
                ItemToBeAdded.CalculateTargetUnit();
                Add(ItemToBeAdded);
                _SelectedItem = ItemToBeAdded;
                return true;
            }
            else return false;
        }// teilweise Contains--> View
        public void DeleteSelectedItem()
        {
            int NewSelectedIndex;

            if ((Count == 0) || (SelectedItem == null)) return;
            if (Count > 1) NewSelectedIndex = IndexOf(_SelectedItem) - 1;
            else NewSelectedIndex = 1;
            Remove(SelectedItem);
            if (Count > 0) _SelectedItem = this[NewSelectedIndex];
            else _SelectedItem = null;
        }
        public void EvaluateMaxID()
        {
            var maxIDFromFile = this.Select(s => s.ID).Max();

            if (maxIDFromFile == null) _MaxID = 0;
            else _MaxID = (long)maxIDFromFile;
        }
        public ShoppingListItemSet OpenSet(string FileName)
        {
            ShoppingListItemSet ReturnUnitSet = this;
            ReturnUnitSet.Clear();
            FileName += FileExtension;
            using (Stream fs = new FileStream(FileName, FileMode.Open))
            {
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(ReturnUnitSet.GetType());
                ReturnUnitSet = (ShoppingListItemSet)x.Deserialize(fs);
            }

            EvaluateMaxID();
            return ReturnUnitSet;

        }// --> Data
        public void SaveSet(string FileName)
        {
            FileName += FileExtension;
            using (FileStream fs = new FileStream(FileName, FileMode.Create))
            {
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(GetType());
                x.Serialize(fs, this);
            }

        }// --> Data
        public ShoppingListItem SelectItem(long IDToSelect)
        {
            ShoppingListItem LocalItemToSelect = new ShoppingListItem();
            LocalItemToSelect.ID = IDToSelect;

            int IndexOfSelectedItem= IndexOf(LocalItemToSelect);
            if (IndexOfSelectedItem == -1) return null;
            else return this[IndexOfSelectedItem];

        }
        public ShoppingListItem SelectItemByIngredient(Ingredient IngredientToBeSearched)
        {
            foreach (ShoppingListItem Item in this)
            {
                if (Item.Ingredient.Equals(IngredientToBeSearched)) return Item;
            }
            return null;
        }            
        public void SetDataReference(IngredientSet IngredientSetData, UnitSet UnitSetData, UnitTranslationSet UnitTranslationSetData)
        {
            _IngredientSetData = IngredientSetData;
            _UnitSetData = UnitSetData;
            _UnitTranslationSetData = UnitTranslationSetData;

            if (this.Count() != 0)
            {
                this[0].SetDataReference(IngredientSetData, UnitSetData, UnitTranslationSetData);
            }
        }
        public override string ToString()
        {
            string ReturnString = "";

            ReturnString += string.Format("\nListe der ShoppingListItems - MaxID: {0}\n", MaxID);
            if (Count == 0) ReturnString += "-------> leer <-------\n";
            else
            {
                var SortedShoppingList = this.OrderBy(s=>s.Ingredient.Name);

                foreach (ShoppingListItem ListItem in SortedShoppingList)
                    ReturnString += ListItem.ToString() + "\n";
            }
            ReturnString += "\n";
            return ReturnString;
        }
    }

}

