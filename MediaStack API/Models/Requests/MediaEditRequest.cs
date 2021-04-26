using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;

namespace MediaStack_API.Models.Requests
{
    public class MediaEditRequest
    {
        #region Properties

        [Required] 
        [Range(1, int.MaxValue)] 
        public int MediaID { get; set; }

        public int? CategoryID { get; set; }

        public int? ArtistID { get; set; }

        public int? AlbumID { get; set; }

        public List<int> TagIDs { get; set; }

        [Range(0, 5)] 
        public int? Score { get; set; }

        public string Source { get; set; }

        #endregion

        #region Methods

        public Media UpdateMedia(IUnitOfWork unitOfWork)
        {
            return null;
        }

        public class BadRequestException : Exception { }

        #endregion
    }
}