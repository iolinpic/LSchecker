// See https://aka.ms/new-console-template for more information
using LSchecker;

LookupRunner lr = new();
await lr.Init();
await lr.runAll();
Console.WriteLine("finish");