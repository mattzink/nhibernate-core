﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2828
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		[Test]
		public async Task WhenPersistShouldNotFetchUninitializedCollectionAsync()
		{
			var companyId = await (CreateScenarioAsync());

			//Now in a second transaction i remove the address and persist Company: for a cascade option the Address will be removed
			using (var sl = new SqlLogSpy())
			{
				using (ISession session = Sfi.OpenSession())
				{
					using (ITransaction tx = session.BeginTransaction())
					{
						var company = await (session.GetAsync<Company>(companyId));
						Assert.That(company.Addresses.Count(), Is.EqualTo(1));
						Assert.That(company.RemoveAddress(company.Addresses.First()), Is.EqualTo(true));

						//now this company will be saved and deleting the address.
						//BUT it should not try to load the BanckAccound collection!
						await (session.PersistAsync(company));
						await (tx.CommitAsync());
					}
				}
				var wholeMessage = sl.GetWholeLog();
				Assert.That(wholeMessage, Does.Not.Contain("BankAccount"));
			}

			await (CleanupAsync(companyId));
		}

		private async Task CleanupAsync(Guid companyId, CancellationToken cancellationToken = default(CancellationToken))
		{
			using (ISession session = Sfi.OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					await (session.DeleteAsync(await (session.GetAsync<Company>(companyId, cancellationToken)), cancellationToken));
					await (tx.CommitAsync(cancellationToken));
				}
			}
		}

		private async Task<Guid> CreateScenarioAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var company = new Company() {Name = "Company test"};
			var address = new Address() {Name = "Address test"};
			var bankAccount = new BankAccount() {Name = "Bank test"};
			company.AddAddress(address);
			company.AddBank(bankAccount);
			using (ISession session = Sfi.OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					await (session.PersistAsync(company, cancellationToken));
					await (tx.CommitAsync(cancellationToken));
				}
			}
			return company.Id;
		}
	}
}
