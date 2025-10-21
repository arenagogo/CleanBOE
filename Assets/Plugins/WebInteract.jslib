mergeInto(LibraryManager.library, {

    IsMobileBrowser: function () {
    return (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent));
  },

  Join: function () {
      document.getElementById("join").click();
  },

  Leave: function () {
      document.getElementById("leave").click();
  },

  Mute: function () {
      document.getElementById("muteAudio").click();
  },

  MuteCam: function () {
      document.getElementById("muteVideo").click();
  },

  Load: function (str) {
      Load(UTF8ToString(str));
  }

});