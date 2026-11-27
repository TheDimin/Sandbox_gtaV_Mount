using System;
using System.Reflection;
using Sandbox;
using Sandbox.Mounting;

namespace GtaV;

internal static class MountTypes
{
	static readonly object RegistrationLock = new();

	internal static void EnsureRegistered()
	{
		var library = Game.TypeLibrary;
		if ( library is null || library.GetType( typeof( GtaWorld ) ) is not null ) return;

		// Older DLL mount loaders discover BaseGameMount but do not register its components.
		lock ( RegistrationLock )
		{
			if ( library.GetType( typeof( GtaWorld ) ) is not null ) return;
			var add = library.GetType().GetMethod( "AddAssembly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				[typeof( Assembly ), typeof( bool )] );
			if ( add is null ) throw new NotSupportedException( "This s&box build cannot register standalone mount components." );
			add.Invoke( library, [typeof( GtaWorld ).Assembly, false] );
		}
	}
}
