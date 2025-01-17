﻿// --------------------------------------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: 2023 Siemens AG
//
//  SPDX-License-Identifier: MIT

// -------------------------------------------------------------------------------------------------------------------- 

using Newtonsoft.Json;

namespace LCT.APICommunications.Model
{
  /// <summary>
  /// The Self model class
  /// </summary>
  public class Self
  {
    [JsonProperty("href")]
    public string Href { get; set; }
  }
}
