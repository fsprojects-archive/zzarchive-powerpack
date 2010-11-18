using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightTester
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private static void FindAndExecuteMethodWithAttribute(Type t, object instance, Type attributeType)
        {
            var methods =
                    from mi in t.GetMethods(BindingFlags.Public)
                    where mi.GetCustomAttributes(attributeType, false).Length > 0
                    select mi;
            var method = methods.FirstOrDefault();
            if (method != null)
                method.Invoke(instance, new object[0]);
        }

        private class Chainer
        {
            private readonly SynchronizationContext sc;

            public Chainer(SynchronizationContext sc)
            {
                this.sc = sc;
            }

        }

        private void Do(SynchronizationContext sc, IEnumerable<Tuple<string, Action>> actions, ref int failures)
        {
            foreach (var a in actions)
            {
                try
                {
                    a.Item2();
                }
                catch (Exception e)
                {
                    failures++;
                    var exn = e is TargetInvocationException ? e.InnerException : e;
                    var message = a.Item1 + " failed:" + exn.Message;
                    sc.Post(state => { testResults.Items.Add(message); }, null);
                    return;
                }
            }
        }

        private void OnRunTests(object sender, RoutedEventArgs e)
        {           
            testResults.Items.Clear();
            testResults.Items.Add("Started running tests " + DateTime.Now);
            var sc = SynchronizationContext.Current;
            button1.IsEnabled = false;
            ThreadPool.QueueUserWorkItem(x =>
            {
                var failures = 0;
                var assembly = typeof(FSharp.PowerPack.Unittests.ArrayTests).Assembly;
                var testFixtures =
                        from type in assembly.GetExportedTypes()
                        where type.GetCustomAttributes(typeof(NUnit.Framework.TestFixtureAttribute), true).Length > 0
                        select type;
                foreach (var fixture in testFixtures)
                {
                    var fixtureName = fixture.Name;
                    if (fixture.GetCustomAttributes(typeof(NUnit.Framework.IgnoreAttribute), true).Length > 0)
                    {
                        sc.Post(state => { testResults.Items.Add(fixtureName + " IGNORED"); }, null);
                        continue;
                    }
                    object o = null;
                    List<Tuple<string, Action>> list = new List<Tuple<string, Action>>();
                    list.Add(Tuple.Create<string, Action>(fixtureName + " activate", () => { o = Activator.CreateInstance(fixture); }));
                    list.Add(Tuple.Create<string, Action>(fixtureName + "setup",
                                () => { FindAndExecuteMethodWithAttribute(fixture, o, typeof(NUnit.Framework.TestFixtureSetUpAttribute)); }));
                    list.AddRange(
                        from mi in fixture.GetMethods()
                        where mi.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute), false).Length > 0
                        select Tuple.Create<string, Action>(
                            fixtureName + "." + mi.Name,
                            () =>
                            {
                                FindAndExecuteMethodWithAttribute(fixture, o, typeof(NUnit.Framework.SetUpAttribute));
                                try
                                {
                                    mi.Invoke(o, new object[0]);
                                }
                                finally
                                {
                                    FindAndExecuteMethodWithAttribute(fixture, o, typeof(NUnit.Framework.TearDownAttribute));
                                }
                                sc.Post(state => { testResults.Items.Add(fixtureName + "." + mi.Name + " passed."); }, null);
                            }
                            ));
                    list.Add(Tuple.Create<string, Action>(fixtureName + "teardown",
                                () => { FindAndExecuteMethodWithAttribute(fixture, o, typeof(NUnit.Framework.TestFixtureTearDownAttribute)); }));


                    Do(sc, list, ref failures);
                }
                sc.Post((state => { button1.IsEnabled = true; testResults.Items.Add("Done. " + failures + " failure(s) seen"); }), null);
            }
            );
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OnRunTests(sender, e);
        }


    }
}
