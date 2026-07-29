using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;

namespace Vega.Services.MasterLibrary;

public class ComponentDefinitionService
{
    private readonly ComponentDefinitionRepository _repository;


    public ComponentDefinitionService()
    {
        _repository = new ComponentDefinitionRepository();
    }


    public List<ComponentDefinition> GetAll()
    {
        return _repository.GetAll();
    }


    public ComponentDefinition? GetById(int id)
    {
        return _repository.GetById(id);
    }


    public void Add(ComponentDefinition component)
    {
        Validate(component);

        _repository.Add(component);
    }


    public void Update(ComponentDefinition component)
    {
        Validate(component);

        _repository.Update(component);
    }


    public void Delete(int id)
    {
        _repository.Delete(id);
    }


    private static void Validate(
        ComponentDefinition component)
    {
        if (string.IsNullOrWhiteSpace(
            component.ManufacturerPartNumber))
        {
            throw new ArgumentException(
                "ManufacturerPartNumber не может быть пустым");
        }


        if (component.PackageId <= 0)
        {
            throw new ArgumentException(
                "PackageId должен быть указан");
        }
    }
}