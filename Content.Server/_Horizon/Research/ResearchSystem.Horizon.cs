using System.Linq;
using Content.Server._Horizon.Research;
using Content.Shared._Horizon.Research;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.Research.Systems;

/// <summary>
/// Horizon: the RnD array is the one research server for everybody. It is reached across grids through RnD relays,
/// and its database holds only the technologies stored on its disks.
/// </summary>
public sealed partial class ResearchSystem
{
    [Dependency] private readonly RndNetworkSystem _rndNetwork = default!;

    /// <summary>
    /// Fills the set with the RnD arrays the client can reach.
    /// </summary>
    /// <returns>False if there is no array at all, so the old servers of the client's grid are used.</returns>
    private bool TryGetArrayServers(EntityUid client, HashSet<Entity<ResearchServerComponent>> servers)
    {
        var found = false;
        var query = EntityQueryEnumerator<RndArrayComponent, ResearchServerComponent>();
        while (query.MoveNext(out var uid, out _, out var server))
        {
            found = true;
            if (_rndNetwork.CanReachArray(client, uid))
                servers.Add((uid, server));
        }

        return found;
    }

    /// <summary>
    /// Whether the client can be connected to the server: an RnD array through the RnD network,
    /// any other server only on the same grid.
    /// </summary>
    private bool CanReachServer(EntityUid client, EntityUid server)
    {
        if (HasComp<RndArrayComponent>(server))
            return _rndNetwork.CanReachArray(client, server);

        return TryComp(client, out TransformComponent? clientXform)
            && TryComp(server, out TransformComponent? serverXform)
            && clientXform.GridUid != null
            && clientXform.GridUid == serverXform.GridUid;
    }

    /// <summary>
    /// Replaces the technologies and the supported disciplines of a database, adding and removing the recipes of the
    /// technologies that came and went, and syncs the clients if it is a server.
    /// </summary>
    public void SetStoredTechnologies(EntityUid uid,
        List<ProtoId<TechnologyPrototype>> technologies,
        List<ProtoId<TechDisciplinePrototype>> disciplines,
        TechnologyDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var removed = component.UnlockedTechnologies.Except(technologies).ToList();
        var added = technologies.Except(component.UnlockedTechnologies).ToList();
        if (removed.Count == 0 && added.Count == 0 && component.SupportedDisciplines.SequenceEqual(disciplines))
            return;

        var recipes = new List<ProtoId<LatheRecipePrototype>>(component.UnlockedRecipes);
        if (removed.Count > 0)
        {
            // A recipe stays while another technology still gives it.
            var kept = new HashSet<ProtoId<LatheRecipePrototype>>();
            foreach (var tech in technologies)
            {
                if (PrototypeManager.TryIndex(tech, out var proto))
                    kept.UnionWith(proto.RecipeUnlocks);
            }

            foreach (var tech in removed)
            {
                if (!PrototypeManager.TryIndex(tech, out var proto))
                    continue;

                foreach (var recipe in proto.RecipeUnlocks)
                {
                    if (!kept.Contains(recipe))
                        recipes.Remove(recipe);
                }
            }
        }

        var addedRecipes = new List<string>();
        foreach (var tech in added)
        {
            if (!PrototypeManager.TryIndex(tech, out var proto))
                continue;

            foreach (var recipe in proto.RecipeUnlocks)
            {
                if (recipes.Contains(recipe))
                    continue;

                recipes.Add(recipe);
                addedRecipes.Add(recipe);
            }
        }

        component.UnlockedTechnologies = new List<ProtoId<TechnologyPrototype>>(technologies);
        component.UnlockedRecipes = recipes;
        component.SupportedDisciplines = new List<ProtoId<TechDisciplinePrototype>>(disciplines);
        Dirty(uid, component);
        UpdateTechnologyCards(uid, component);

        // The clients hold the lists of the server, so they have to be given the new ones.
        if (TryComp<ResearchServerComponent>(uid, out var server))
        {
            foreach (var client in server.Clients)
            {
                SyncClientWithServer(client);
            }
        }

        var ev = new TechnologyDatabaseModifiedEvent(addedRecipes);
        RaiseLocalEvent(uid, ref ev);
    }
}
