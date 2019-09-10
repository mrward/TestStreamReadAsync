//
// ViewController.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2019 Microsoft Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace TestStreamReadAsync
{
	public partial class ViewController : NSViewController
	{
		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			RunTask ().ContinueWith (t => {
				if (t.Exception != null)
					Console.WriteLine (t.Exception);
			});
		}

		async Task RunTask ()
		{
			int originalThreadId = Thread.CurrentThread.ManagedThreadId;
			Console.WriteLine ("ThreadId={0}", Thread.CurrentThread.ManagedThreadId);

			var fileName = Path.GetTempFileName ();
			File.WriteAllText (fileName, "test");

			using (Stream stream = File.OpenRead (fileName)) {
				int count = (int)stream.Length;
				byte[] buf = new byte[count];

				// UI thread before.
				await stream.ReadAsync (buf, 0, count);

				// Should continue on UI thread but continues on non-UI thread

				// Using Task.Run instead of above ReadAsync code works.
				//await Task.Run (() => {
				//	Thread.Sleep (400);
				//});
			}

			File.Delete (fileName);

			int newThreadId = Thread.CurrentThread.ManagedThreadId;
			Console.WriteLine ("ThreadId={0}", Thread.CurrentThread.ManagedThreadId);

			InvokeOnMainThread (() => {
				var alert = new NSAlert ();
				alert.MessageText = string.Format ("OriginalThreadId={0} After await: ThreadId={1}", originalThreadId, newThreadId);
				alert.AddButton ("OK");

				var result = alert.RunModal ();
			});
		}

		public override NSObject RepresentedObject {
			get {
				return base.RepresentedObject;
			}
			set {
				base.RepresentedObject = value;
				// Update the view, if already loaded.
			}
		}
	}
}
