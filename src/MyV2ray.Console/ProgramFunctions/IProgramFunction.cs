namespace MyV2ray.Console.ProgramFunctions
{
    interface IProgramFunction
    {
        static string FunctionName { get; }

        void Execute();
    }
}
