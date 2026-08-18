using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;

namespace Vega.Services.MasterLibrary;

public sealed class ComponentTapeReelGeometryService
{
    private readonly ComponentTapeReelGeometryRepository _repository;
    private readonly ComponentDefinitionRepository _componentRepository;

    public ComponentTapeReelGeometryService()
        : this(new ComponentTapeReelGeometryRepository(), new ComponentDefinitionRepository())
    {
    }

    internal ComponentTapeReelGeometryService(
        ComponentTapeReelGeometryRepository repository,
        ComponentDefinitionRepository componentRepository)
    {
        _repository = repository;
        _componentRepository = componentRepository;
    }

    public List<ComponentTapeReelGeometry> GetProfiles(int componentDefinitionId) =>
        componentDefinitionId <= 0 ? [] : _repository.GetByComponentId(componentDefinitionId);

    public ComponentTapeReelGeometry? GetDefaultProfile(int componentDefinitionId) =>
        componentDefinitionId <= 0 ? null : _repository.GetDefaultByComponentId(componentDefinitionId);

    public ComponentTapeReelGeometry CreateProfile(ComponentTapeReelGeometry profile)
    {
        Validate(profile);
        NormalizePickupRotation(profile);
        return _repository.Create(profile);
    }

    public void UpdateProfile(ComponentTapeReelGeometry profile)
    {
        if (profile.Id <= 0)
            throw new ArgumentException("Tape-and-reel geometry Id must be specified.");

        Validate(profile);
        NormalizePickupRotation(profile);
        _repository.Update(profile);
    }

    public void DeleteProfile(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Tape-and-reel geometry Id must be specified.");

        _repository.Delete(id);
    }

    public void SetDefaultProfile(int componentDefinitionId, int id)
    {
        if (componentDefinitionId <= 0)
            throw new ArgumentException("ComponentDefinitionId must be greater than zero.");
        if (id <= 0)
            throw new ArgumentException("Tape-and-reel geometry Id must be specified.");
        EnsureComponentExists(componentDefinitionId);
        _repository.SetDefault(componentDefinitionId, id);
    }

    private void Validate(ComponentTapeReelGeometry profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (profile.ComponentDefinitionId <= 0)
            throw new ArgumentException("ComponentDefinitionId must be greater than zero.");

        EnsureComponentExists(profile.ComponentDefinitionId);

        if (profile.CarrierTapeWidth < 0 ||
            profile.PocketPitch < 0 ||
            profile.PocketLength < 0 ||
            profile.PocketWidth < 0 ||
            profile.PocketDepth < 0 ||
            profile.SprocketHolePitch < 0 ||
            profile.SprocketHoleDiameter < 0 ||
            profile.ReelDiameter < 0 ||
            profile.HubDiameter < 0 ||
            profile.QuantityPerReel < 0)
        {
            throw new ArgumentException("Tape-and-reel dimensions and quantity cannot be negative.");
        }

        if (profile.CarrierTapeWidth > 0 && profile.PocketPitch <= 0)
            throw new ArgumentException("PocketPitch must be greater than zero when CarrierTapeWidth is specified.");

        if (!Enum.IsDefined(profile.FeedDirection))
            throw new ArgumentException("FeedDirection is invalid.");
        if (!Enum.IsDefined(profile.PocketOrientation))
            throw new ArgumentException("PocketOrientation is invalid.");
        if (!Enum.IsDefined(profile.VerificationStatus))
            throw new ArgumentException("VerificationStatus is invalid.");
    }

    private void EnsureComponentExists(int componentDefinitionId)
    {
        if (_componentRepository.GetById(componentDefinitionId) is null)
            throw new ArgumentException("ComponentDefinition was not found.");
    }

    private static void NormalizePickupRotation(ComponentTapeReelGeometry profile)
    {
        profile.PickupRotation %= 360;
        if (profile.PickupRotation < 0)
            profile.PickupRotation += 360;
    }
}
