﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Diagnostics.DataProviders;
using Diagnostics.DataProviders.Interfaces;
using Diagnostics.RuntimeHost.Models;
using Xunit;

namespace Diagnostics.Tests.DataProviderTests
{
    public class KustoMapTests
    {
        IKustoMap emptyKustoMap;
        IKustoMap publicAzureSampleKustoMap;
        IKustoMap governmentAzureSampleKustoMap;
        IKustoMap brokenKustoMap;

        public KustoMapTests()
        {
            Setup();
        }

        private void Setup()
        {
            emptyKustoMap = new KustoMap(DataProviderConstants.AzureCloud);

            var existingmap1 = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    { $"{DataProviderConstants.AzureCloudAlternativeName}ClusterName", "fake_cluster1" },
                    { $"{DataProviderConstants.AzureCloudAlternativeName}DatabaseName", "fake_database1" },
                    { $"{DataProviderConstants.AzureChinaCloudCodename}ClusterName", "fake_cluster2" },
                    { $"{DataProviderConstants.AzureChinaCloudCodename}DatabaseName", "fake_database2" },
                    { $"{DataProviderConstants.AzureUSGovernmentCodename}ClusterName", "fake_cluster3" },
                    { $"{DataProviderConstants.AzureUSGovernmentCodename}DatabaseName", "fake_database3" }
                }
            };

            var existingmap2 = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    { $"{DataProviderConstants.AzureCloudAlternativeName}ClusterName", "fake_cluster1" },
                    { $"{DataProviderConstants.AzureCloudAlternativeName}DatabaseName", null },
                    { $"{DataProviderConstants.AzureChinaCloudCodename}ClusterName", "fake_cluster2" },
                    { $"{DataProviderConstants.AzureChinaCloudCodename}DatabaseName", "fake_database2" },
                    { $"{DataProviderConstants.AzureUSGovernmentCodename}ClusterName", "fake_cluster3" },
                    { $"{DataProviderConstants.AzureUSGovernmentCodename}DatabaseName", "fake_database3" }
                }
            };

            publicAzureSampleKustoMap = new KustoMap(DataProviderConstants.AzureCloudAlternativeName, existingmap1);
            governmentAzureSampleKustoMap = new KustoMap(DataProviderConstants.AzureUSGovernmentCodename, existingmap1);
            brokenKustoMap = new KustoMap(DataProviderConstants.AzureCloudAlternativeName, existingmap2);
        }

        [Fact]
        public void TestEmptyData()
        {
            Assert.Equal(null, emptyKustoMap.MapCluster("fake_cluster"));
            Assert.Equal(null, emptyKustoMap.MapDatabase("fake_database"));
        }

        [Fact]
        public void TestReflexive()
        {
            var targetCluster = publicAzureSampleKustoMap.MapCluster("fake_cluster1");
            Assert.Equal("fake_cluster1", targetCluster);

            Assert.Equal("fake_cluster3", governmentAzureSampleKustoMap.MapCluster("fake_cluster3"));
        }

        [Fact]
        public void TestGettingClusterCounterpart()
        {
            publicAzureSampleKustoMap.TryGetDatabase(new KustoClusterEntry("fake_cluster2"), out string targetDatabase2);
            Assert.True("fake_database1".Equals(targetDatabase2, StringComparison.CurrentCultureIgnoreCase));

            Assert.Equal("fake_cluster3", governmentAzureSampleKustoMap.MapCluster("fake_cluster2"));
        }

        [Fact]
        public void TestAddingNullOrEmptyClustersAndDatabases()
        {
            Assert.False(brokenKustoMap.TryGetDatabase(new KustoClusterEntry("fake_cluster1"), out string targetDatabase));
        }

        [Fact]
        public void TestEveryClusterHasItsOwnDatabase()
        {
            publicAzureSampleKustoMap.TryGetDatabase(new KustoClusterEntry("fake_cluster1"), out string targetDatabase);
            Assert.Equal("fake_database1", targetDatabase);

            publicAzureSampleKustoMap.TryGetCluster(new KustoDatabaseEntry("fake_database1"), out string targetCluster);
            Assert.Equal("fake_cluster1", targetCluster);
        }

        [Fact]
        public void TestMappedClusterIsNullAndNotEmptyString()
        {
            Assert.Null(publicAzureSampleKustoMap.MapCluster("does_not_exist"));
            Assert.Null(publicAzureSampleKustoMap.MapDatabase("does_not_exist"));
        }
    }
}