using Csla;
using Csla.Data;
using System;
using System.Linq;
using Csla.Serialization;
using System.Collections.Generic;

namespace ProjectTracker.Library
{
  [Serializable()]
  public class ResourceList : ReadOnlyListBase<ResourceList, ResourceInfo>
  {
    public static ResourceList EmptyList()
    {
      return new ResourceList();
    }

    public static void GetResourceList(EventHandler<DataPortalResult<ResourceList>> callback)
    {
      DataPortal.BeginFetch<ResourceList>(callback);
    }

#if !SILVERLIGHT
    public static ResourceList GetResourceList()
    {
      return DataPortal.Fetch<ResourceList>();
    }

    private void DataPortal_Fetch()
    {
      var rlce = RaiseListChangedEvents;
      RaiseListChangedEvents = false;
      IsReadOnly = false;
      using (var ctx = ProjectTracker.Dal.DalFactory.GetManager())
      {
        var dal = ctx.GetProvider<ProjectTracker.Dal.IResourceDal>();
        List<ProjectTracker.Dal.ResourceDto> list = null;
        list = dal.Fetch();
        foreach (var item in list)
          Add(DataPortal.FetchChild<ResourceInfo>(item));
      }
      IsReadOnly = true;
      RaiseListChangedEvents = rlce;
    }
#endif
  }
}