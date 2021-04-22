#region Copyright
// openLAMA is an open source platform which has been developed by the
// Swiss Kanton Basel Landschaft, with the goal of automating and managing
// large scale Covid testing programs or any other pandemic/viral infections.

// Copyright(C) 2021 Kanton Basel Landschaft, Switzerland
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// See LICENSE.md in the project root for license information.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see https://www.gnu.org/licenses/.
#endregion

using System.Collections.Generic;
using Elyon.Fastly.EmailJob.DomainServices.Properties;

namespace Elyon.Fastly.EmailJob.DomainServices.AttachmentFiles
{
    public static class AttachmentsByTypes
    {
        public static List<AttachmentData> CompanyOnboarding()
        {
            var attachments = new List<AttachmentData>();
            var file = new AttachmentData { FileName = "01 Schulungspräsentation_compressed.pdf" };
            file.SetContent(Resources._01_Schulungspräsentation_compressed);
            attachments.Add(file);

            file = new AttachmentData { FileName = "02 Informationsflyer_compressed.pdf" };
            file.SetContent(Resources._02_Informationsflyer_compressed);
            attachments.Add(file);

            file = new AttachmentData { FileName = "03 Offizielles Informationsschreiben des Kanton Baselland.pdf" };
            file.SetContent(Resources._03_Offizielles_Informationsschreiben_des_Kanton_Baselland);
            attachments.Add(file);

            file = new AttachmentData { FileName = "04 Vereinbarung zwischen Kanton und Betrieb.pdf" };
            file.SetContent(Resources._04_Vereinbarung_zwischen_Kanton_und_Betrieb);
            attachments.Add(file);

            file = new AttachmentData { FileName = "05 Einverständniserklärung für den Teilnehmenden (Erwachsene).pdf" };
            file.SetContent(Resources._05_Einverständniserklärung_für_den_Teilnehmenden__Erwachsene_);
            attachments.Add(file);

            file = new AttachmentData { FileName = "05 Einverständniserklärung für den Teilnehmenden (Minderjährige).pdf" };
            file.SetContent(Resources._05_Einverständniserklärung_für_den_Teilnehmenden__Minderjährige_);
            attachments.Add(file);

            file = new AttachmentData { FileName = "06 Anmeldeformular Depooling für die Teilnehmenden (Erwachsene).pdf" };
            file.SetContent(Resources._06_Anmeldeformular_Depooling_für_die_Teilnehmenden__Erwachsene_);
            attachments.Add(file);

            file = new AttachmentData { FileName = "06 Anmeldeformular Depooling für die Teilnehmenden (Minderjährige).pdf" };
            file.SetContent(Resources._06_Anmeldeformular_Depooling_für_die_Teilnehmenden__Minderjährige_);
            attachments.Add(file);

            file = new AttachmentData { FileName = "07 Anfahrtsplan Labor Muttenz.pdf" };
            file.SetContent(Resources._07_Anfahrtsplan_Labor_Muttenz);
            attachments.Add(file);

            file = new AttachmentData { FileName = "Handbuch BTBL BBL_KMU.pdf" };
            file.SetContent(Resources.Handbuch_BTBL_BBL_KMU);
            attachments.Add(file);

            return attachments;
        }
    }
}
