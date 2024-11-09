namespace SsmsTableDependencyDiagram.Application.Interfaces
{
    public interface IDiagramGeneratorService
    {
        void GenerateDiagram(bool isCompact = true);
    }
}