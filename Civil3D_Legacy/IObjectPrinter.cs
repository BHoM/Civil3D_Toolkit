namespace Civil3D_Toolkit
{
    public interface IObjectPrinter<in T>
    {
        string[] Print(T printableObject);
    }
}
