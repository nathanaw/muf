﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MonitoredUndo;
using MonitoredUndo.Tests;
using System.Threading;

namespace MonitoredUndo.Tests
{

    [TestClass]
    public class Undo_Test
    {

        
        public RootDocument Document1 { get; set; }
        public RootDocument Document2 { get; set; }

        [TestInitialize]
        public void TestSetup()
        {
            Document1 = new RootDocument();
            Document1.A = new ChildA() { Name = "Document1.ChildA", UndoableSometimes = "Value1" };
            Document1.Bs.Add(new ChildB() { Name = "Document1.ChildB[0]" });
            Document1.Bs.Add(new ChildB() { Name = "Document1.ChildB[1]" });
            Document1.Bs.Add(new ChildB() { Name = "Document1.ChildB[2]" });

            Document1.KeyValuePairs[0] = new ChildA() { Name = "Document1.ChildA.0" };
            Document1.KeyValuePairs[1] = new ChildA() { Name = "Document1.ChildA.1" };
            Document1.KeyValuePairs[2] = new ChildA() { Name = "Document1.ChildA.2" };

            Assert.IsNotNull(Document1.A.Root);
            Assert.AreSame(Document1, Document1.A.Root);
            Assert.AreSame(Document1, Document1.Bs[0].Root);
            Assert.AreSame(Document1, Document1.Bs[1].Root);
            Assert.AreSame(Document1, Document1.Bs[2].Root);

            Document2 = new RootDocument();
            Document2.A = new ChildA() { Name = "Document2.ChildA", UndoableSometimes = "Value2" };
            Document2.Bs.Add(new ChildB() { Name = "Document2.ChildB[0]" });
            Document2.Bs.Add(new ChildB() { Name = "Document2.ChildB[1]" });
            Document2.Bs.Add(new ChildB() { Name = "Document2.ChildB[2]" });

            UndoService.Current.Clear();
        }

        

        
        [TestMethod]
        public void UndoService_Creates_UndoRoot_For_Document()
        {
            var undoRoot = UndoService.Current[Document1];
            Assert.IsNotNull(undoRoot);
        }

        [TestMethod]
        public void UndoService_Returns_Same_Root()
        {
            var undoRoot1 = UndoService.Current[Document1];
            var undoRoot2 = UndoService.Current[Document1];
            Assert.IsNotNull(undoRoot1);
            Assert.IsNotNull(undoRoot2);
            Assert.IsTrue(ReferenceEquals(undoRoot1, undoRoot2));
        }
        
        [TestMethod]
        public void UndoService_Uses_WeakReferences()
        {
            // Create a root, but don't keep any references to the object that it is for.
            UndoRoot undoRoot = null;
            WeakReference wrDoc = GetWeakReference(out undoRoot);

            // Confirm the object is still alive.
            Assert.IsNotNull(undoRoot);
            Assert.IsTrue(wrDoc.IsAlive);

            // Force a GC to collect the document used for the undo root.
            // Don't do this in normal code... this is just for testing.
            GC.Collect(1, GCCollectionMode.Forced, true);

            // Confirm the RootDocument was collected.
            // This shows that we're not leaking memory via the UndoService's _Roots dictionary.
            Assert.IsFalse(wrDoc.IsAlive);
            Assert.IsNull(wrDoc.Target);


            // Inline function.
            // This method is part of UndoService_Uses_WeakReferences.
            // Code is in a separate method so that the variables are properly scoped
            // which allows the GC call to work properly.
            WeakReference GetWeakReference(out UndoRoot undoRoot)
            {
                var document = new RootDocument();          // Constructed, but not returned.
                undoRoot = UndoService.Current[document];   // This creates a weakly referenced undo root.
                return new WeakReference(document);         // Return a weak reference so that we can test if the document was collected.
            }
        }

        [TestMethod]
        public void UndoService_Supports_Multiple_Root_Documents()
        {
            var undoRoot1 = UndoService.Current[Document1];
            var undoRoot2 = UndoService.Current[Document2];

            Assert.IsNotNull(undoRoot1);
            Assert.IsNotNull(undoRoot2);
            Assert.AreNotSame(undoRoot1, undoRoot2);
        }

        [TestMethod]
        public void UndoService_Has_Current_Property_for_Singleton_Instance()
        {
            UndoService svc = UndoService.Current;
            UndoService svc2 = UndoService.Current;

            Assert.IsNotNull(svc);
            Assert.IsNotNull(svc2);
            Assert.AreSame(svc, svc2);
        }

        [TestMethod]
        public void UndoService_Does_Not_Support_Null_Document()
        {
            var root = UndoService.Current[null];
            Assert.IsNull(root);
        }

        [TestMethod]
        public void UndoRoot_Supports_Adding_ChangeSets()
        {
            Document1.A.Name = "Updated1";
            Document1.A.Name = "Updated2";

            Assert.AreEqual(2, UndoService.Current[Document1].UndoStack.Count());
        }

        [TestMethod]
        public void UndoRoot_Supports_Adding_ChangeSets_With_Description()
        {
            Document1.A.Name = "Updated1";
            Document1.A.Name = "Updated2";

            Assert.AreEqual(2, UndoService.Current[Document1].UndoStack.Count());

            var change = UndoService.Current[Document1].UndoStack.FirstOrDefault();
            Assert.AreEqual("Name changed.", change.Description);
        }

        [TestMethod]
        public void UndoRoot_Supports_Adding_Collection_ChangeSets_With_Description()
        {
            Document1.Bs.Add(new ChildB() { Name = "Document1.ChildB[3]" });

            Assert.AreEqual(1, UndoService.Current[Document1].UndoStack.Count());

            var change = UndoService.Current[Document1].UndoStack.FirstOrDefault();
            Assert.AreEqual("Collection of B's Changed", change.Description);
        }

        [TestMethod]
        public void UndoRoot_Has_CanUndo_Property()
        {
            Assert.AreEqual(false, UndoService.Current[Document1].CanUndo);

            Document1.A.Name = "Updated1";

            Assert.AreEqual(true, UndoService.Current[Document1].CanUndo);

            UndoService.Current[Document1].Undo();

            Assert.AreEqual(false, UndoService.Current[Document1].CanUndo);
        }

        [TestMethod]
        public void UndoRoot_Has_CanRedo_Property()
        {
            Assert.AreEqual(false, UndoService.Current[Document1].CanRedo);

            Document1.A.Name = "Updated1";

            Assert.AreEqual(false, UndoService.Current[Document1].CanRedo);

            UndoService.Current[Document1].Undo();

            Assert.AreEqual(true, UndoService.Current[Document1].CanRedo);

            UndoService.Current[Document1].Redo();

            Assert.AreEqual(false, UndoService.Current[Document1].CanRedo);
        }

        [TestMethod]
        public void UndoRoot_Supports_Adding_ChangeSets_Directly()
        {
            var change = new DelegateChange(Document1.A,
                                    () => Document1.A.Name = "Original",
                                    () => Document1.A.Name = "NewValue",
                                    new Tuple<object, string>(Document1.A, "Name"));

            var undoRoot = UndoService.Current[Document1];

            // var changeSet = new ChangeSet(undoRoot, "Change Document.A.Name", change);

            undoRoot.AddChange(change, "Change Document.A.Name");

            Assert.AreEqual(1, undoRoot.UndoStack.Count());
            Assert.AreEqual(0, undoRoot.RedoStack.Count());

            var origValue = Document1.A.Name;

            undoRoot.Undo();
            Assert.AreEqual("Original", Document1.A.Name);

            undoRoot.Redo();
            Assert.AreEqual("NewValue", Document1.A.Name);
        }

        [TestMethod]
        public void UndoRoot_Prunes_Redo_Stack_When_Adding_New_ChangeSet()
        {
            var orig = Document1.A.Name;

            Document1.A.Name = "Updated1";
            Document1.A.Name = "Updated2";

            var undoRoot = UndoService.Current[Document1];

            Assert.AreEqual(2, undoRoot.UndoStack.Count());
            Assert.AreEqual(0, undoRoot.RedoStack.Count());

            undoRoot.Undo();
            undoRoot.Undo();
            Assert.AreEqual(orig, Document1.A.Name);
            Assert.AreEqual(2, undoRoot.RedoStack.Count());

            Document1.A.Name = "Updated3";

            Assert.AreEqual(1, undoRoot.UndoStack.Count());
            Assert.AreEqual(0, undoRoot.RedoStack.Count());
        }

        [TestMethod]
        public void UndoRoot_Raises_UndoStackChanged_Event_When_ChangeSet_Added()
        {
            var orig = Document1.A.Name;
            var undoRoot = UndoService.Current[Document1];
            var wasCalled = false;

            var callback = new EventHandler((s, e) => wasCalled = true);
            undoRoot.UndoStackChanged += callback;

            try
            {
                Document1.A.Name = "Updated1";
                Assert.IsTrue(wasCalled);

                wasCalled = false;

                undoRoot.Undo();
                Assert.IsTrue(wasCalled);

                wasCalled = false;

                undoRoot.Redo();
                Assert.IsTrue(wasCalled);
            }
            finally
            {
                undoRoot.UndoStackChanged -= callback;
            }
        }

        [TestMethod]
        public void UndoRoot_Raises_RedoStackChanged_Event_When_ChangeSet_Added()
        {
            var orig = Document1.A.Name;
            var undoRoot = UndoService.Current[Document1];
            var wasCalled = false;

            var callback = new EventHandler((s, e) => wasCalled = true);
            undoRoot.RedoStackChanged += callback;

            try
            {
                Document1.A.Name = "Updated1";
                Assert.IsTrue(wasCalled, "Redo stack event should have been called because of the new change pruning the redo stack.");

                wasCalled = false;

                undoRoot.Undo();
                Assert.IsTrue(wasCalled);

                wasCalled = false;

                undoRoot.Redo();
                Assert.IsTrue(wasCalled);
            }
            finally
            {
                undoRoot.UndoStackChanged -= callback;
            }
        }

        [TestMethod]
        public void UndoRoot_UndoStack_Implements_INotifyCollectionChanged()
        {
            string orig = Document1.A.Name;
            var undoRoot = UndoService.Current[Document1];
            Assert.IsInstanceOfType(undoRoot.UndoStack, typeof(INotifyCollectionChanged));

            bool wasCalledForAdd = false;
            bool wasCalledForRemove = false;
            bool wasCalledForReset = false;

            var callback = new NotifyCollectionChangedEventHandler((s, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        wasCalledForAdd = true;
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        wasCalledForRemove = true;
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        wasCalledForReset = true;
                        break;
                }
            });

            ((INotifyCollectionChanged)undoRoot.UndoStack).CollectionChanged += callback;

            try
            {
                Document1.A.Name = "Updated1";
                Assert.IsTrue(wasCalledForAdd);
                Assert.IsFalse(wasCalledForRemove);
                Assert.IsFalse(wasCalledForReset);

                wasCalledForAdd = false;

                undoRoot.Undo();
                Assert.IsTrue(wasCalledForRemove);
                Assert.IsFalse(wasCalledForAdd);
                Assert.IsFalse(wasCalledForReset);

                wasCalledForRemove = false;

                undoRoot.Redo();
                Assert.IsTrue(wasCalledForAdd);
                Assert.IsFalse(wasCalledForRemove);
                Assert.IsFalse(wasCalledForReset);

                wasCalledForAdd = false;

                undoRoot.Clear();
                Assert.IsTrue(wasCalledForReset);
                Assert.IsFalse(wasCalledForAdd);
                Assert.IsFalse(wasCalledForRemove);
            }
            finally
            {
                ((INotifyCollectionChanged)undoRoot.UndoStack).CollectionChanged -= callback;
            }
        }

        [TestMethod]
        public void UndoRoot_RedoStack_Implements_INotifyCollectionChanged()
        {
            string orig = Document1.A.Name;
            var undoRoot = UndoService.Current[Document1];
            Assert.IsInstanceOfType(undoRoot.RedoStack, typeof(INotifyCollectionChanged));

            bool wasCalledForAdd = false;
            bool wasCalledForRemove = false;
            bool wasCalledForReset = false;

            var callback = new NotifyCollectionChangedEventHandler((s, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        wasCalledForAdd = true;
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        wasCalledForRemove = true;
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        wasCalledForReset = true;
                        break;
                }
            });

            ((INotifyCollectionChanged)undoRoot.RedoStack).CollectionChanged += callback;

            try
            {
                Document1.A.Name = "Updated1";
                Assert.IsTrue(wasCalledForReset, "Redo stack CollectionChanged event should have been called with NotifyCollectionChangedAction.Reset args because of the new change pruning the redo stack.");
                Assert.IsFalse(wasCalledForAdd);
                Assert.IsFalse(wasCalledForRemove);

                wasCalledForReset = false;

                undoRoot.Undo();
                Assert.IsTrue(wasCalledForAdd);
                Assert.IsFalse(wasCalledForRemove);
                Assert.IsFalse(wasCalledForReset);

                wasCalledForAdd = false;

                undoRoot.Redo();
                Assert.IsTrue(wasCalledForRemove);
                Assert.IsFalse(wasCalledForAdd);
                Assert.IsFalse(wasCalledForReset);

                wasCalledForRemove = false;

                undoRoot.Clear();
                Assert.IsTrue(wasCalledForReset);
                Assert.IsFalse(wasCalledForAdd);
                Assert.IsFalse(wasCalledForRemove);
            }
            finally
            {
                ((INotifyCollectionChanged)undoRoot.RedoStack).CollectionChanged -= callback;
            }
        }

        [TestMethod]
        public void UndoRoot_Can_Undo_the_Last_ChangeSet()
        {
            var orig = Document1.A.Name;
            var firstChange = "First Change";
            var secondChange = "Second Change";

            Document1.A.Name = firstChange;
            Document1.A.Name = secondChange;

            UndoService.Current[Document1].Undo();

            Assert.AreEqual(firstChange, Document1.A.Name);
        }

        [TestMethod]
        public void UndoRoot_Can_Undo_the_Last_ChangeSet_With_Conditional_Undo_On_A_Property()
        {
            var orig = Document1.A.Name;
            var firstChange = "DISABLE_UNDO";
            var secondChange = "New Value";

            Document1.A.Name = firstChange;
            Document1.A.UndoableSometimes = secondChange;

            UndoService.Current[Document1].Undo();

            Assert.AreEqual(firstChange, Document1.A.Name);
            Assert.AreEqual(secondChange, Document1.A.UndoableSometimes);
        }

        [TestMethod]
        public void UndoRoot_Can_Redo_the_Last_ChangeSet()
        {
            var orig = Document1.A.Name;
            var firstChange = "First Change";
            var secondChange = "Second Change";

            Document1.A.Name = firstChange;
            Document1.A.Name = secondChange;

            UndoService.Current[Document1].Undo();

            Assert.AreEqual(firstChange, Document1.A.Name);

            UndoService.Current[Document1].Redo();

            Assert.AreEqual(secondChange, Document1.A.Name);
        }

        [TestMethod]
        public void UndoRoot_Can_Undo_Multiple_ChangeSets()
        {
            var orig = Document1.A.Name;
            var firstChange = "First Change";
            var secondChange = "Second Change";
            var root = UndoService.Current[Document1];

            Document1.A.Name = firstChange;
            Document1.A.Name = secondChange;

            Assert.AreEqual(2, root.UndoStack.Count());
            Assert.AreEqual(0, root.RedoStack.Count());

            root.Undo(root.UndoStack.Last());

            Assert.AreEqual(orig, Document1.A.Name);
            Assert.AreEqual(0, root.UndoStack.Count());
            Assert.AreEqual(2, root.RedoStack.Count());
        }

        [TestMethod]
        public void UndoRoot_Can_Redo_Multiple_ChangeSets()
        {
            var orig = Document1.A.Name;
            var firstChange = "First Change";
            var secondChange = "Second Change";
            var root = UndoService.Current[Document1];

            Document1.A.Name = firstChange;
            Document1.A.Name = secondChange;

            Assert.AreEqual(2, root.UndoStack.Count());
            Assert.AreEqual(0, root.RedoStack.Count());

            root.Undo();
            root.Undo();

            Assert.AreEqual(orig, Document1.A.Name);
            Assert.AreEqual(0, root.UndoStack.Count());
            Assert.AreEqual(2, root.RedoStack.Count());

            root.Redo(root.RedoStack.Last());

            Assert.AreEqual(secondChange, Document1.A.Name);
            Assert.AreEqual(2, root.UndoStack.Count());
            Assert.AreEqual(0, root.RedoStack.Count());
        }

        [TestMethod]
        public void UndoRoot_Supports_Starting_a_Batch_Of_Changes()
        {
            var orig = Document1.A.Name;
            var firstChange = "First Change";
            var secondChange = "Second Change";
            var root = UndoService.Current[Document1];

            using (new UndoBatch(Document1, "Change Name", false))
            {
                Document1.A.Name = firstChange;
                Document1.A.Name = secondChange;
            }

            Assert.AreEqual(1, root.UndoStack.Count());
            Assert.AreEqual(0, root.RedoStack.Count());

            root.Undo();

            Assert.AreEqual(orig, Document1.A.Name);
            Assert.AreEqual(0, root.UndoStack.Count());
            Assert.AreEqual(1, root.RedoStack.Count());

            root.Redo();

            Assert.AreEqual(secondChange, Document1.A.Name);
            Assert.AreEqual(1, root.UndoStack.Count());
            Assert.AreEqual(0, root.RedoStack.Count());
        }

        [TestMethod]
        public void UndoRoot_Supports_Nested_Batches_Of_Changes()
        {
            var orig = Document1.A.Name;
            var orig2 = Document1.Bs[0].Name;
            var firstChange = "First Change";
            var secondChange = "Second Change";
            var thirdChange = "Third Change";
            var fourthChange = "Fourth Change";
            var root = UndoService.Current[Document1];

            using (new UndoBatch(Document1, "Change Name", false))
            {
                Document1.A.Name = firstChange;
                Document1.A.Name = secondChange;

                using (new UndoBatch(Document1, "Change Collection", false))
                {
                    Document1.Bs[0].Name = thirdChange;
                    Document1.Bs[0].Name = fourthChange;
                }
            }

            Assert.AreEqual(secondChange, Document1.A.Name);
            Assert.AreEqual(fourthChange, Document1.Bs[0].Name);
            Assert.AreEqual(1, root.UndoStack.Count());
            Assert.AreEqual(0, root.RedoStack.Count());

            root.Undo();

            Assert.AreEqual(orig, Document1.A.Name);
            Assert.AreEqual(orig2, Document1.Bs[0].Name);
            Assert.AreEqual(0, root.UndoStack.Count());
            Assert.AreEqual(1, root.RedoStack.Count());

            root.Redo();

            Assert.AreEqual(secondChange, Document1.A.Name);
            Assert.AreEqual(fourthChange, Document1.Bs[0].Name);
            Assert.AreEqual(1, root.UndoStack.Count());
            Assert.AreEqual(0, root.RedoStack.Count());
        }

        [TestMethod]
        public void ChangeSet_Supports_Consolidating_Changes_For_The_Same_Change_Key()
        {
            var orig = Document1.A.Name;
            var orig2 = Document1.Bs[0].Name;
            var firstChange = "First Change";
            var secondChange = "Second Change";
            var thirdChange = "Third Change";
            var fourthChange = "Fourth Change";
            var root = UndoService.Current[Document1];

            using (new UndoBatch(Document1, "Change Name", true))
            {
                Document1.A.Name = firstChange;
                Document1.A.Name = secondChange;

                using (new UndoBatch(Document1, "Change Collection", true))
                {
                    Document1.Bs[0].Name = thirdChange;
                    Document1.Bs[0].Name = fourthChange;
                }
            }

            Assert.AreEqual(secondChange, Document1.A.Name);
            Assert.AreEqual(fourthChange, Document1.Bs[0].Name);
            Assert.AreEqual(1, root.UndoStack.Count());
            Assert.AreEqual(0, root.RedoStack.Count());

            root.Undo();

            Assert.AreEqual(orig, Document1.A.Name);
            Assert.AreEqual(orig2, Document1.Bs[0].Name);
            Assert.AreEqual(0, root.UndoStack.Count());
            Assert.AreEqual(1, root.RedoStack.Count());

            root.Redo();

            Assert.AreEqual(secondChange, Document1.A.Name);
            Assert.AreEqual(fourthChange, Document1.Bs[0].Name);
            Assert.AreEqual(1, root.UndoStack.Count());
            Assert.AreEqual(0, root.RedoStack.Count());
        }

        [TestMethod]
        public void DefaultChangeFactory_Supports_Collection_Adds()
        {
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[2].Name);

            Document1.Bs.Insert(2, new ChildB() { Name = "Document1.ChildB[2a]" });

            Assert.AreEqual(4, Document1.Bs.Count);

            UndoService.Current[Document1].Undo();

            Assert.AreEqual(3, Document1.Bs.Count);
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[2].Name);

            UndoService.Current[Document1].Redo();

            Assert.AreEqual(4, Document1.Bs.Count);
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2a]", Document1.Bs[2].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[3].Name);
        }

        [TestMethod]
        public void DefaultChangeFactory_Supports_Collection_MultipleAdds()
        {
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[2].Name);

            using (new UndoBatch(Document1, "Multiple Adds", false))
            {
                Document1.Bs.Insert(2, new ChildB() { Name = "Document1.ChildB[2a]" });
                Document1.Bs.Insert(2, new ChildB() { Name = "Document1.ChildB[2b]" });
                Document1.Bs.Add(new ChildB() { Name = "Document1.ChildB[2c]" });
            }

            Assert.AreEqual(6, Document1.Bs.Count);
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2b]", Document1.Bs[2].Name);
            Assert.AreEqual("Document1.ChildB[2a]", Document1.Bs[3].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[4].Name);
            Assert.AreEqual("Document1.ChildB[2c]", Document1.Bs[5].Name);

            UndoService.Current[Document1].Undo();

            Assert.AreEqual(3, Document1.Bs.Count);
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[2].Name);

            UndoService.Current[Document1].Redo();

            Assert.AreEqual(6, Document1.Bs.Count);
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2b]", Document1.Bs[2].Name);
            Assert.AreEqual("Document1.ChildB[2a]", Document1.Bs[3].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[4].Name);
            Assert.AreEqual("Document1.ChildB[2c]", Document1.Bs[5].Name);
        }

        [TestMethod]
        public void DefaultChangeFactory_Supports_Collection_Removes()
        {
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[2].Name);

            Document1.Bs.RemoveAt(1);

            Assert.AreEqual(2, Document1.Bs.Count);

            UndoService.Current[Document1].Undo();

            Assert.AreEqual(3, Document1.Bs.Count);
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[2].Name);

            UndoService.Current[Document1].Redo();

            Assert.AreEqual(2, Document1.Bs.Count);
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[1].Name);
        }

        [TestMethod]
        public void DefaultChangeFactory_Supports_Collection_MulitpleRemoves()
        {
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[2].Name);

            using (new UndoBatch(Document1, "Multiple Removes", false))
            {
                Document1.Bs.RemoveAt(0);
                Document1.Bs.RemoveAt(1);
                Document1.Bs.RemoveAt(0);
            }

            Assert.AreEqual(0, Document1.Bs.Count);

            UndoService.Current[Document1].Undo();

            Assert.AreEqual(3, Document1.Bs.Count);
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[2].Name);

            UndoService.Current[Document1].Redo();

            Assert.AreEqual(0, Document1.Bs.Count);
        }

        [TestMethod]
        public void DefaultChangeFactory_Supports_Collection_Reset()
        {
            try
            {
                DefaultChangeFactory.Current.ThrowExceptionOnCollectionResets = true;
                Document1.Bs.Clear();
                Assert.Fail("Should have throw NotSupportedException");
            }
            catch (NotSupportedException) {}

            // Nothing added to undo stack.
            Assert.AreEqual(0, UndoService.Current[Document1].UndoStack.Count());

            // Repopulate the collection, and then clear the undo stack.
            Document1.Bs.Add(new ChildB() { Name = "New B1" });
            Document1.Bs.Add(new ChildB() { Name = "New B2" });
            UndoService.Current[Document1].Clear();
            
            DefaultChangeFactory.Current.ThrowExceptionOnCollectionResets = false;
            Document1.Bs.Clear();

            // Nothing added to undo stack.
            Assert.AreEqual(0, UndoService.Current[Document1].UndoStack.Count());
            Assert.AreEqual(0, Document1.Bs.Count);

            UndoService.Current[Document1].Undo();
            Assert.AreEqual(0, Document1.Bs.Count);
        }

        [TestMethod]
        public void DefaultChangeFactory_Supports_Dictionary_Adds()
        {
            Assert.AreEqual("Document1.ChildA.0", Document1.KeyValuePairs[0].Name);
            Assert.AreEqual("Document1.ChildA.1", Document1.KeyValuePairs[1].Name);
            Assert.AreEqual("Document1.ChildA.2", Document1.KeyValuePairs[2].Name);

            Document1.KeyValuePairs[3] = new ChildA() { Name = "Document1.ChildA.3" };

            Assert.AreEqual(4, Document1.KeyValuePairs.Count);

            UndoService.Current[Document1].Undo();

            Assert.AreEqual(3, Document1.KeyValuePairs.Count);
            Assert.AreEqual("Document1.ChildA.0", Document1.KeyValuePairs[0].Name);
            Assert.AreEqual("Document1.ChildA.1", Document1.KeyValuePairs[1].Name);
            Assert.AreEqual("Document1.ChildA.2", Document1.KeyValuePairs[2].Name);

            UndoService.Current[Document1].Redo();

            Assert.AreEqual(4, Document1.KeyValuePairs.Count);
            Assert.AreEqual("Document1.ChildA.0", Document1.KeyValuePairs[0].Name);
            Assert.AreEqual("Document1.ChildA.1", Document1.KeyValuePairs[1].Name);
            Assert.AreEqual("Document1.ChildA.2", Document1.KeyValuePairs[2].Name);
            Assert.AreEqual("Document1.ChildA.3", Document1.KeyValuePairs[3].Name);
        }

        [TestMethod]
        public void DefaultChangeFactory_Supports_Dictionary_Removes()
        {
            Assert.AreEqual("Document1.ChildA.0", Document1.KeyValuePairs[0].Name);
            Assert.AreEqual("Document1.ChildA.1", Document1.KeyValuePairs[1].Name);
            Assert.AreEqual("Document1.ChildA.2", Document1.KeyValuePairs[2].Name);

            Document1.KeyValuePairs.Remove(1);

            Assert.AreEqual(2, Document1.KeyValuePairs.Count);

            UndoService.Current[Document1].Undo();

            Assert.AreEqual(3, Document1.KeyValuePairs.Count);
            Assert.AreEqual("Document1.ChildA.0", Document1.KeyValuePairs[0].Name);
            Assert.AreEqual("Document1.ChildA.1", Document1.KeyValuePairs[1].Name);
            Assert.AreEqual("Document1.ChildA.2", Document1.KeyValuePairs[2].Name);

            UndoService.Current[Document1].Redo();

            Assert.AreEqual(2, Document1.KeyValuePairs.Count);
            Assert.AreEqual("Document1.ChildA.0", Document1.KeyValuePairs[0].Name);
            Assert.AreEqual("Document1.ChildA.2", Document1.KeyValuePairs[2].Name);
        }

        [TestMethod]
        public void DefaultChangeFactory_Supports_Dictionary_Replace()
        {
            Assert.AreEqual("Document1.ChildA.0", Document1.KeyValuePairs[0].Name);
            Assert.AreEqual("Document1.ChildA.1", Document1.KeyValuePairs[1].Name);
            Assert.AreEqual("Document1.ChildA.2", Document1.KeyValuePairs[2].Name);

            Document1.KeyValuePairs[2] = new ChildA() { Name = "Document1.ChildA.2.A" };

            Assert.AreEqual(3, Document1.KeyValuePairs.Count);

            UndoService.Current[Document1].Undo();

            Assert.AreEqual(3, Document1.KeyValuePairs.Count);
            Assert.AreEqual("Document1.ChildA.0", Document1.KeyValuePairs[0].Name);
            Assert.AreEqual("Document1.ChildA.1", Document1.KeyValuePairs[1].Name);
            Assert.AreEqual("Document1.ChildA.2", Document1.KeyValuePairs[2].Name);

            UndoService.Current[Document1].Redo();

            Assert.AreEqual(3, Document1.KeyValuePairs.Count);
            Assert.AreEqual("Document1.ChildA.0", Document1.KeyValuePairs[0].Name);
            Assert.AreEqual("Document1.ChildA.1", Document1.KeyValuePairs[1].Name);
            Assert.AreEqual("Document1.ChildA.2.A", Document1.KeyValuePairs[2].Name);
        }

        [TestMethod]
        [Ignore] // TODO
        public void DefaultChangeFactory_Supports_Dictionary_MultipleRemoves()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        [Ignore] // TODO
        public void DefaultChangeFactory_Supports_Dictionary_MultipleAdds()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void DefaultChangeFactory_Supports_Collection_Move()
        {
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[2].Name);

            Document1.Bs.Move(0, 1);

            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[2].Name);

            UndoService.Current[Document1].Undo();

            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[2].Name);
        }

        [TestMethod]
        public void DefaultChangeFactory_Supports_Collection_Replace()
        {
            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[2].Name);

            var newB = new ChildB() { Name = "New B" };
            Document1.Bs[1] = newB;

            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("New B", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[2].Name);

            UndoService.Current[Document1].Undo();

            Assert.AreEqual("Document1.ChildB[0]", Document1.Bs[0].Name);
            Assert.AreEqual("Document1.ChildB[1]", Document1.Bs[1].Name);
            Assert.AreEqual("Document1.ChildB[2]", Document1.Bs[2].Name);
        }


        [TestMethod]
        public void ChangeKey_Of_Objects_Are_Equal()
        {
            var o1 = new object();
            var o2 = new object();

            CompareChangeKeys(o1, o2, o1, o2);
        }

        [TestMethod]
        public void ChangeKey_Of_Object_And_Int_Are_Equal()
        {
            var o1 = new object();
            CompareChangeKeys(o1, 5, o1, 5);
        }

        [TestMethod]
        public void ChangeKey_Of_Int_And_Int_Are_Equal()
        {
            CompareChangeKeys(1, 5, 1, 5);
        }

        [TestMethod]
        public void ChangeKey_Of_String_And_Int_Are_Equal()
        {
            CompareChangeKeys("One", 5, "One", 5);
        }

        [TestMethod]
        public void ChangeKey_Of_Different_Objects_Are_Equal()
        {
            try
            {
                var o1 = new object();
                var o2 = new object();
                var o3 = new object();

                CompareChangeKeys(o1, o2, o3, o2);

                Assert.Fail("Expected failed assertion");
            }
            catch { }
        }

        [TestMethod]
        public void ChangeKey_Of_Different_Object_And_Int_Are_Equal()
        {
            try
            {
                var o1 = new object();
                CompareChangeKeys(o1, 5, o1, 10);

                Assert.Fail("Expected failed assertion");
            }
            catch { }
        }

        [TestMethod]
        public void ChangeKey_Of_Different_Int_And_Int_Are_Equal()
        {
            try
            {
                CompareChangeKeys(1, 5, 2, 5);

                Assert.Fail("Expected failed assertion");
            }
            catch { }
        }

        [TestMethod]
        public void ChangeKey_Of_Different_String_And_Int_Are_Equal()
        {
            try
            {
                CompareChangeKeys("One", 5, "Two", 5);

                Assert.Fail("Expected failed assertion");
            }
            catch { }
        }


        [TestMethod]
        public void WeakReferences_Are_Equal()
        {
            var testObject = new object();
            var wRef1 = new WeakReference(testObject);
            var wRef2 = new WeakReference(testObject);
            var comparer = new WeakReferenceComparer();

            Assert.AreNotEqual(wRef1, wRef2);
            Assert.IsFalse(wRef1.Equals(wRef2));
            Assert.IsFalse(wRef1 == wRef2);

            Assert.IsTrue(comparer.Equals(wRef1, wRef2));
            Assert.IsFalse(comparer.Equals(null, wRef2));
            Assert.IsFalse(comparer.Equals(wRef1, null));
        }



        private static void CompareChangeKeys<T1, T2>(T1 a1, T2 a2, T1 b1, T2 b2)
        {
            var key1 = new ChangeKey<T1, T2>(a1, a2);
            var key2 = new ChangeKey<T1, T2>(b1, b2);

            Assert.AreEqual(key1, key2);
            Assert.AreEqual(key1, key2);
            Assert.IsTrue(key1.Equals(key2));
            Assert.AreEqual(key1.GetHashCode(), key2.GetHashCode());
            Assert.AreEqual(key1.ToString(), key2.ToString());
        }        

    }
}
