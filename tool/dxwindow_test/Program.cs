// See https://aka.ms/new-console-template for more information
using dxwindow_test;

Console.WriteLine("Hello, World!");

var window = new TestWindow();
window.Dock = DockStyle.Fill;
var form = new Form();
form.Controls.Add(window);
form.ShowDialog();