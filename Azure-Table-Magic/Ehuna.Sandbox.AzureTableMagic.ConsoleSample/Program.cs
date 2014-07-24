// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="@ehuna">
// Copyright © @ehuna.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
// </copyright>
//  <notes>

//             .=., 
//            ;c =\ 
//          __|  _/ 
//        .'-'-._/-'-._ 
//       /..   ____    \ 
//      /' _  [<_->] )  \ 
//     (  / \--\_>/-/'._ ) 
//      \-;_/\__;__/ _/ _/ 
//       '._}|==o==\{_\/ 
//        /  /-._.--\  \_ 
//       // /   /|   \ \ \ 
//      / | |   | \;  |  \ \ 
//     / /  | :/   \: \   \_\ 
//    /  |  /.'|   /: |    \ \ 
//    |  |  |--| . |--|     \_\ 
//    / _/   \ | : | /___--._) \ 
//   |_(---'-| >-'-| |       '-' 
// hero     /_/     \_\
//
//  Authors: Emmanuel Huna
//  License: The MIT License (MIT) https://github.com/ehuna/ehuna-sandbox/blob/master/LICENSE 
//
//  </notes>
// -----------------------------------------------------------------------

using System;
using System.Configuration;
using System.Linq.Expressions;
using Ehuna.Sandbox.AzureTableMagic.ConsoleSample.Models;
using Ehuna.Sandbox.AzureTableMagic.Storage.Table;

namespace Ehuna.Sandbox.AzureTableMagic.ConsoleSample
{
    internal class Program
    {
        // Chabge me in App.config
        private static readonly string ConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
        private static readonly Guid TheMerchantId = Guid.NewGuid();

        private static void Main(string[] args)
        {
            Console.WriteLine("AzureTableMagic Console Demo v1.0");
            Console.WriteLine("by @ehuna (c) 2014, All Rights Reserved.");
            Console.WriteLine("");

            System.Net.ServicePointManager.DefaultConnectionLimit = 35;
            TableExample();

            Console.WriteLine("Press any key to close...");
            Console.ReadLine();
        }

        private static void TableExample()
        {
            var azureTableRepository = new AzureTableRepository<FileProgressTable>(
                ConnectionString,
                partitionKeyGetters: new Expression<Func<FileProgressTable, object>>[] { 
                                        item => item.MerchantId},
                rowKeyGetters:      new Expression<Func<FileProgressTable, object>>[] {
                                        item => item.FileId});

            var progress = new FileProgressTable {
                MerchantId = TheMerchantId,
                FileId = Guid.NewGuid().ToString("N"),
                FileSizeInBytes = 1000000,
                FileBytesProcessed = 0
            };

            azureTableRepository.InsertOrReplace(progress);
        }
    }
}
