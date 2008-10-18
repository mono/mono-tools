// 
// Gendarme.Framework.Engines.OpCodeEngine
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
//

using System;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework.Helpers;

namespace Gendarme.Framework.Engines {

	/// <summary>
	/// Build an OpCodeBitmask for each MethodBody. By using the extra data gathered
	/// by this engine it is possible to short-circuit rule logic early by ensuring
	/// it does what the rule expects (e.g. check for [call|callvirt] instructions).
	/// </summary>
	public class OpCodeEngine : Engine {

		public OpCodeEngine ()
		{
		}

		public override void Initialize (EngineController controller)
		{
			base.Initialize (controller);
			controller.BuildingMethodBody += new EventHandler<EngineEventArgs> (OnMethodBody);
		}

		void OnMethodBody (object sender, EngineEventArgs e)
		{
			MethodBody body = (sender as MethodBody);
			IAnnotationProvider ap = (body.Method as IAnnotationProvider);
			// some runners, like the wizard, can run this engine several times
			// and, in this case, the result won't change
			if (ap.Annotations.Contains ("OPCODEBITMASK"))
				return;

			OpCodeBitmask mask = new OpCodeBitmask ();
			foreach (Instruction ins in body.Instructions) {
				mask.Set (ins.OpCode.Code);
			}
			ap.Annotations.Add ("OPCODEBITMASK", mask);
		}

		// service offered by the engine

		static public OpCodeBitmask GetBitmask (MethodDefinition method)
		{
			IAnnotationProvider ap = (method as IAnnotationProvider);
			OpCodeBitmask result = (OpCodeBitmask) ap.Annotations ["OPCODEBITMASK"];
			if (result == null)
				return OpCodeBitmask.All;
			return result;
		}
	}
}
