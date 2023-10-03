namespace test_bench
{
    interface ITest 
    {
        string TestName { get; }

        void Run();
    }
}