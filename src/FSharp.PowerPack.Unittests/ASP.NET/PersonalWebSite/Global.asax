<%@ Application Language="F#" %>
<script runat="server">

    member this.Application_Start (sender:obj, e:EventArgs) =
        SiteMap.add_SiteMapResolve(new SiteMapResolveEventHandler(this.AppendQueryString));
        if (not (Roles.RoleExists("Administrators"))) then Roles.CreateRole("Administrators");
        if (not (Roles.RoleExists("Friends"))) then Roles.CreateRole("Friends");
    
    member this.AppendQueryString (o:obj) (e:SiteMapResolveEventArgs) =
        if (SiteMap.CurrentNode <> null) then
            let temp = SiteMap.CurrentNode.Clone(true);
            let u = new Uri(e.Context.Request.Url.ToString());
            temp.Url <- temp.Url + u.Query;
            if (temp.ParentNode <> null) then
                temp.ParentNode.Url <- temp.ParentNode.Url + u.Query;
            temp;
        else 
            null;
    
</script>
