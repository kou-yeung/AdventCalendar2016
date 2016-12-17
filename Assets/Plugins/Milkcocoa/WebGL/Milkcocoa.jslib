//=====================================================
// Milkcocoa Plugin for UnityWebGL
//=====================================================

//========================================
// Milkcocoa Instance.
var LibraryMilkcocoa = {
	$MilkcocoaInstance : [],
	$DataStoreInstance : [],
    MilkcocoaCreate: function (host) {
		var milkcocoa = new MilkCocoa(Pointer_stringify(host));
		return MilkcocoaInstance.push(milkcocoa) - 1;
    },
	MilkcocoaDestroy: function (id) {
		var milkcocoa = MilkcocoaInstance[id];
		if(milkcocoa == null) return;
		milkcocoa.logout();
		MilkcocoaInstance[id] = null;
    },
	MilkcocoaDataStore: function (id, path) {
		var milkcocoa = MilkcocoaInstance[id];
		if(milkcocoa == null) return -1;
		var dataStore = milkcocoa.dataStore(Pointer_stringify(path));
		return DataStoreInstance.push(dataStore) - 1;
    },
	DataStoreSend: function (id, json) {
		var dataStore = DataStoreInstance[id];
		if(dataStore == null) return;
		dataStore.send(JSON.parse(Pointer_stringify(json)));
    },
	DataStoreAddSendEvent: function (id, onsend) {
		var dataStore = DataStoreInstance[id];
		if(dataStore == null) return;
		dataStore.on("send", function(sent){
			var sentValue = allocate(intArrayFromString(JSON.stringify(sent.value)), 'i8', ALLOC_STACK);
			Runtime.dynCall('vii', onsend, [id, sentValue]);
		});
	},
};
autoAddDeps(LibraryMilkcocoa, '$MilkcocoaInstance');
autoAddDeps(LibraryMilkcocoa, '$DataStoreInstance');
mergeInto(LibraryManager.library, LibraryMilkcocoa);

//========================================
// Installer for load script.
var LibraryMilkcocoaInstaller = {
    $MilkcocoaInstaller: {
        script:null,
	    installed: false,
	    Install: function () {
	        if (MilkcocoaInstaller.script != null) return;

	        MilkcocoaInstaller.script = document.createElement("script");
	        MilkcocoaInstaller.script.addEventListener("load", function (e) {
	            MilkcocoaInstaller.installed = true;
	        });
	        MilkcocoaInstaller.script.src = "https://cdn.mlkcca.com/v0.6.0/milkcocoa.js";
	        document.body.appendChild(MilkcocoaInstaller.script);
	    },
    },
    MilkcocoaInstall: function () {
        MilkcocoaInstaller.Install();
    },
    MilkcocoaInstalled:function() {
        return MilkcocoaInstaller.installed;
    },
};

autoAddDeps(LibraryMilkcocoaInstaller, '$MilkcocoaInstaller');
mergeInto(LibraryManager.library, LibraryMilkcocoaInstaller);

