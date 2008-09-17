﻿using Csla;
using Csla.DataPortalClient;
using System;
using Csla.Serialization;
using UnitDriven;

#if NUNIT
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestSetup = NUnit.Framework.SetUpAttribute;
#elif MSTEST
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace cslalighttest.CslaDataProvider
{
#if TESTING
  [System.Diagnostics.DebuggerNonUserCode]
#endif
  [TestClass]
  public class CslaDataProviderTest : TestBase
  {
    [TestInitialize]
    public void Setup()
    {
      DataPortal.ProxyTypeName = typeof(SynchronizedWcfProxy<>).AssemblyQualifiedName;
      WcfProxy.DefaultUrl = cslalighttest.Properties.Resources.RemotePortalUrl;
    }

    [TestMethod]
    public void When_Create_instantiates_Customer_with_random_id_between_1_and_10_DataSource_receives_that_record()
    {
      var context = GetContext();

      var provider = new Csla.Silverlight.CslaDataProvider();
      provider.PropertyChanged += (o1, e1) =>
      {
        if (e1.PropertyName == "Data")
        {
          var customer = (Customer)provider.Data;
          context.Assert.AreEqual(true, customer.Id > 0 && customer.Id < 11);
          context.Assert.Success();
        }
      };
      provider.IsInitialLoadEnabled = true;
      provider.ManageObjectLifetime = true;
      provider.FactoryMethod = "CreateCustomer";
      provider.ObjectType = typeof(Customer).AssemblyQualifiedName;//"cslalighttest.CslaDataProvider.Customer, Csla.Testing.Business, Version=..., Culture=neutral, PublicKeyToken=null";

      context.Complete();
    }

    [TestMethod]
    public void When_Fetch_with_no_parameters_loads_Customer_with_random_id_between_1_and_10_DataSource_receives_that_record()
    {
      var context = GetContext();

      var provider = new Csla.Silverlight.CslaDataProvider();
      provider.PropertyChanged += (o1, e1) =>
      {
        if (e1.PropertyName == "Data")
        {
          var customer = (Customer)provider.Data;
          context.Assert.AreEqual(true, customer.Id > 0 && customer.Id<11);
          context.Assert.Success();
        }
      };
      provider.IsInitialLoadEnabled = true;
      provider.ManageObjectLifetime = true;
      provider.FactoryMethod = "GetCustomer";
      provider.ObjectType = typeof(Customer).AssemblyQualifiedName;//"cslalighttest.CslaDataProvider.Customer, Csla.Testing.Business, Version=..., Culture=neutral, PublicKeyToken=null";

      context.Complete();
    }

    [TestMethod]
    public void When_Fetch_called_with_random_value_between_1_and_10_parameter_DataSource_receives_that_record()
    {
      var context = GetContext();

      int custId = (new Random()).Next(1, 10);
      var provider = new Csla.Silverlight.CslaDataProvider();
      provider.PropertyChanged += (o1, e1) =>
      {
        if (e1.PropertyName == "Data")
        {
          context.Assert.AreEqual(custId, ((Customer)provider.Data).Id);
          context.Assert.Success();
        }
      };
      provider.IsInitialLoadEnabled = true;
      provider.ManageObjectLifetime = true;
      provider.FactoryParameters.Add(custId);
      provider.FactoryMethod = "GetCustomer";
      provider.ObjectType = typeof(Customer).AssemblyQualifiedName;//"cslalighttest.CslaDataProvider.Customer, Csla.Testing.Business, Version=..., Culture=neutral, PublicKeyToken=null";

      context.Complete();
    }

    [TestMethod]
    public void Cancel_reverts_property_values_on_bound_BO_back_to_the_original_values()
    {
      var context = GetContext();

      var provider = new Csla.Silverlight.CslaDataProvider();
      Customer.GetCustomer((o1,e1) =>
      {
        var cust = e1.Object;
        int custID = cust.Id;
        string custName = cust.Name;
        provider.Data = cust;
        cust.Name = "new test name";
        provider.Cancel();
        context.Assert.AreEqual(custID, ((Customer)provider.Data).Id);
        context.Assert.AreEqual(custName, ((Customer)provider.Data).Name);
        context.Assert.Success();
      });
      context.Complete();
    }

    [TestMethod]
    public void TestCslaDataProviderSave()
    {
      var context = GetContext();

      var provider = new Csla.Silverlight.CslaDataProvider();
      Customer.GetCustomer((o1, e1) =>
      {
        Csla.ApplicationContext.GlobalContext.Clear();
        var cust = e1.Object;
        int custID = cust.Id;
        string custName = cust.Name;
        provider.Data = cust;
        cust.Name = "new test name";
        provider.PropertyChanged += (o2, e2) =>
        {
          if (e2.PropertyName == "Data")
          {
            context.Assert.AreEqual("Updating Customer new test name", ((Customer)provider.Data).Method);
            context.Assert.Success();
          }
        };
        provider.Save();
      });
      context.Complete();
    }

    [TestMethod]
    public void If_Fetch_returns_X_items_and_then_DataSource_removes_one_and_adds_two_Count_should_be_X_plus_1()
    {
      var context = GetContext();

      var provider = new Csla.Silverlight.CslaDataProvider();
      provider.ManageObjectLifetime = true;
      CustomerList.GetCustomerList((o1, e1) =>
      {
        Csla.ApplicationContext.GlobalContext.Clear();
        var custs = e1.Object;
        int count = custs.Count;
        provider.Data = custs;
        provider.RemoveItem(custs[0]);
        provider.AddNewItem();
        provider.AddNewItem();
        context.Assert.AreEqual(count -1 + 2,custs.Count);
        context.Assert.Success();

      });
      context.Complete();
    }

    [TestMethod]
    public void IF_BO_Throws_Exception_DataSource_Error_property_contains_Exception_info()
    {
      var context = GetContext();

      var provider = new Csla.Silverlight.CslaDataProvider();
      provider.PropertyChanged += (o1, e1) =>
      {
        if (e1.PropertyName == "Error")
        {
          context.Assert.IsNotNull(provider.Error);
          context.Assert.Success();
        }
      };
      provider.IsInitialLoadEnabled = true;
      provider.ManageObjectLifetime = true;
      provider.FactoryMethod = "GetCustomerWithException";
      provider.ObjectType = typeof(Customer).AssemblyQualifiedName;//"cslalighttest.CslaDataProvider.Customer, Csla.Testing.Business, Version=..., Culture=neutral, PublicKeyToken=null";

      context.Complete();
      
    }

    [TestMethod]
    public void Refresh_Calls_FactoryMethod_second_time()
    {
      var context = GetContext();
      int dataLoadedNTimes = 0;
      var provider = new Csla.Silverlight.CslaDataProvider();
      provider.PropertyChanged += (o1, e1) =>
      {
        if (e1.PropertyName == "Data" && ++dataLoadedNTimes==2)
        {
          context.Assert.Success();
        }
      };
      provider.IsInitialLoadEnabled = true;
      provider.ManageObjectLifetime = true;
      provider.FactoryMethod = "GetCustomer";
      provider.ObjectType = typeof(Customer).AssemblyQualifiedName;//"cslalighttest.CslaDataProvider.Customer, Csla.Testing.Business, Version=..., Culture=neutral, PublicKeyToken=null";

      //Second call
      provider.Refresh();

      context.Complete();

    }
    [TestMethod]
    public void Fetch_call_on_BO_that_does_not_implement_DP_Fetch_returns_Exception_info_in_Error_property()
    {
      var context = GetContext();

      var provider = new Csla.Silverlight.CslaDataProvider();
      provider.PropertyChanged += (o1, e1) =>
      {
        if (e1.PropertyName == "Error")
        {
          context.Assert.IsNotNull(provider.Error);
          context.Assert.Success();
        }
      };
      provider.IsInitialLoadEnabled = true;
      provider.ManageObjectLifetime = true;
      provider.FactoryMethod = "GetCustomer";
      provider.ObjectType = typeof (CustomerWO_DP_XYZ).AssemblyQualifiedName;// "cslalighttest.CslaDataProvider.CustomerWO_DP_XYZ, Csla.Testing.Business, Version=..., Culture=neutral, PublicKeyToken=null";

      context.Complete();
      
    }

    [TestMethod]
    public void Create_call_on_BO_that_does_not_implement_DP_Create_returns_Exception_info_in_Error_property()
    {
      var context = GetContext();

      var provider = new Csla.Silverlight.CslaDataProvider();
      provider.PropertyChanged += (o1, e1) =>
      {
        if (e1.PropertyName == "Error")
        {
          context.Assert.IsNotNull(provider.Error);
          context.Assert.Success();
        }
      };
      provider.IsInitialLoadEnabled = true;
      provider.ManageObjectLifetime = true;
      provider.FactoryMethod = "CreateCustomer";
      provider.ObjectType = typeof(CustomerWO_DP_XYZ).AssemblyQualifiedName;//"cslalighttest.CslaDataProvider.CustomerWO_DP_XYZ, Csla.Testing.Business, Version=..., Culture=neutral, PublicKeyToken=null";

      context.Complete();

    }

  }

}
